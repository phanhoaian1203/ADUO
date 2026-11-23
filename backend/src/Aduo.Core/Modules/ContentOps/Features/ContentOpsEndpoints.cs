using Aduo.Core.Common.Content;
using Aduo.Core.Common.Http;
using Aduo.Core.Common.Results;
using Aduo.Core.Common.Security;
using Aduo.Core.Modules.ContentOps.Domain;
using Aduo.Core.Modules.ContentOps.Services;
using Aduo.Core.Modules.Identity.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Aduo.Core.Modules.ContentOps.Features;

public sealed record LifecycleRequest(int Version, string? Note = null);

public sealed record PublishRequest(int Version, ChecklistRequest? Checklist);

public sealed record BatchPublishRequest(IReadOnlyList<BatchPublishItem>? Items, ChecklistRequest? Checklist);

public sealed record UpdateSettingsRequest(Dictionary<string, decimal>? Values);

public static class ContentOpsEndpoints
{
    public static IEndpointRouteBuilder MapContentOpsEndpoints(this IEndpointRouteBuilder app)
    {
        // Công khai: trang ghi công /nguon-du-lieu (nghĩa vụ của CC BY và CC BY-SA).
        app.MapGet("/api/v1/attributions", async (ReviewQueueService s, CancellationToken ct) => Results.Ok(await s.AttributionsAsync(ct)))
            .WithTags("ContentOps").Produces<IReadOnlyList<AttributionItem>>();

        var admin = app.MapGroup("/api/v1/admin").WithTags("ContentOps Admin").RequireAdmin(Permissions.ContentManage);
        MapSourcesAndSettings(admin);
        MapMedia(admin);
        MapImports(admin);
        MapReviewAndLifecycle(admin);

        // Publish cần quyền riêng content.publish (BR-CNT-05): có thể tách người soạn và người duyệt về sau mà không đổi code.
        var publish = app.MapGroup("/api/v1/admin").WithTags("ContentOps Admin").RequireAdmin(Permissions.ContentPublish);
        publish.MapPost("/{resource}/{id:guid}/publish", async (
            string resource, Guid id, PublishRequest req, LifecycleService s, HttpContext ctx, CancellationToken ct)
            => (await s.PublishAsync(resource, id, req.Version, req.Checklist, ctx.CurrentUserId(), ct)).ToHttpResult())
            .Produces<ContentActionResponse>();
        publish.MapPost("/review-queue/publish-batch", async (
            BatchPublishRequest req, ReviewQueueService s, HttpContext ctx, CancellationToken ct) =>
        {
            if (req.Items is not { Count: > 0 })
            {
                return Error.Validation(ErrorCodes.ValidationFailed, "Chưa chọn mục nào để publish.").ToProblem();
            }

            return Results.Ok(await s.PublishBatchAsync(req.Items, req.Checklist, ctx.CurrentUserId(), ct));
        }).Produces<IReadOnlyList<BatchPublishOutcome>>();

        return app;
    }

    private static void MapSourcesAndSettings(RouteGroupBuilder g)
    {
        g.MapGet("/licenses", async (SourceService s, CancellationToken ct) => Results.Ok(await s.ListLicensesAsync(ct)))
            .Produces<IReadOnlyList<LicenseResponse>>();
        g.MapGet("/sources", async (SourceService s, CancellationToken ct) => Results.Ok(await s.ListAsync(ct)))
            .Produces<IReadOnlyList<SourceResponse>>();
        g.MapPost("/sources", async (CreateSourceRequest req, SourceService s, CancellationToken ct)
            => (await s.CreateAsync(req, ct)).ToHttpResult(v => Results.Created($"/api/v1/admin/sources/{v.Id}", v)))
            .Produces<SourceResponse>(StatusCodes.Status201Created);
        g.MapPatch("/sources/{id:guid}", async (Guid id, UpdateSourceRequest req, SourceService s, CancellationToken ct)
            => (await s.UpdateAsync(id, req, ct)).ToHttpResult())
            .Produces<SourceResponse>();

        g.MapGet("/settings", async (SettingsService s, CancellationToken ct) => Results.Ok(await s.ListAsync(ct)))
            .Produces<IReadOnlyList<SettingResponse>>();
        g.MapPatch("/settings", async (UpdateSettingsRequest req, SettingsService s, CancellationToken ct) =>
        {
            if (req.Values is not { Count: > 0 })
            {
                return Error.Validation(ErrorCodes.ValidationFailed, "Chưa có giá trị nào để cập nhật.").ToProblem();
            }

            return (await s.UpdateAsync(req.Values, ct)).ToHttpResult();
        }).Produces<IReadOnlyList<SettingResponse>>();
    }

    private static void MapMedia(RouteGroupBuilder g)
    {
        // Form multipart: dùng CSRF riêng của hệ thống (double-submit), nên tắt antiforgery mặc định của ASP.NET Core.
        g.MapPost("/media", async (HttpRequest req, MediaService media, HttpContext ctx, CancellationToken ct) =>
        {
            var form = await ReadFormAsync(req, ct);
            var file = form?.Files.GetFile("file");
            if (form is null || file is null || !Guid.TryParse(form["sourceId"], out var sourceId))
            {
                return Error.Validation(ErrorCodes.ValidationFailed, "Cần chọn file và nguồn.").ToProblem();
            }

            short? licenseId = short.TryParse(form["licenseId"], out var l) ? l : null;
            await using var stream = file.OpenReadStream();
            var result = await media.UploadAsync(
                new UploadMediaRequest(file.FileName, stream, sourceId, licenseId, form["attribution"], form["originalUrl"], ctx.CurrentUserId()), ct);

            return result.IsSuccess
                ? Results.Json(new UploadResponse(MediaQueries.ToResponse(result.Value.Media), result.Value.Reused),
                    statusCode: result.Value.Reused ? StatusCodes.Status200OK : StatusCodes.Status201Created)
                : result.Error.ToProblem();
        }).DisableAntiforgery().Produces<UploadResponse>(StatusCodes.Status201Created);

        g.MapGet("/media", async (MediaQueries q, MediaKind? kind, string? search, int? page, int? pageSize, CancellationToken ct)
            => Results.Ok(await q.ListAsync(kind, search, page ?? 1, Math.Clamp(pageSize ?? 24, 1, 100), ct))).Produces<MediaPage>();

        g.MapGet("/media/{id:guid}/file", async (Guid id, MediaQueries q, CancellationToken ct) =>
        {
            var file = await q.OpenAsync(id, ct);
            return file is null ? Error.NotFound().ToProblem() : Results.File(file.Value.Stream, file.Value.ContentType, enableRangeProcessing: true);
        });
    }

    private static void MapImports(RouteGroupBuilder g)
    {
        g.MapGet("/imports", async (ImportService s, int? page, int? pageSize, CancellationToken ct)
            => Results.Ok(await s.ListAsync(page ?? 1, Math.Clamp(pageSize ?? 20, 1, 100), ct))).Produces<IReadOnlyList<ImportBatchResponse>>();

        g.MapGet("/imports/content-types", (ImportService s) => Results.Ok(s.ContentTypes)).Produces<IReadOnlyList<string>>();

        g.MapGet("/imports/templates/{contentType}", (string contentType, string? format, ImportService s) =>
        {
            var handler = s.Find(contentType);
            if (handler is null)
            {
                return ImportService.UnknownContentType.ToProblem();
            }

            var t = handler.GetTemplate(format ?? "csv");
            return Results.File(System.Text.Encoding.UTF8.GetBytes(t.Content), t.ContentType, t.FileName);
        });

        // Dry-run: nhận file, KHÔNG ghi bản ghi nội dung, trả báo cáo (hợp lệ / lỗi / trùng).
        g.MapPost("/imports", async (HttpRequest req, ImportService s, HttpContext ctx, CancellationToken ct) =>
        {
            var form = await ReadFormAsync(req, ct);
            var file = form?.Files.GetFile("file");
            if (form is null || file is null || !Guid.TryParse(form["sourceId"], out var sourceId) || string.IsNullOrWhiteSpace(form["contentType"]))
            {
                return Error.Validation(ErrorCodes.ValidationFailed, "Cần chọn loại nội dung, nguồn và file.").ToProblem();
            }

            await using var stream = file.OpenReadStream();
            return (await s.DryRunAsync(form["contentType"].ToString(), sourceId, file.FileName, stream, ctx.CurrentUserId(), ct))
                .ToHttpResult(v => Results.Created($"/api/v1/admin/imports/{v.Id}", v));
        }).DisableAntiforgery().Produces<ImportBatchResponse>(StatusCodes.Status201Created);

        g.MapGet("/imports/{id:guid}", async (Guid id, ImportService s, CancellationToken ct) => (await s.GetAsync(id, ct)).ToHttpResult())
            .Produces<ImportBatchResponse>();
        g.MapPost("/imports/{id:guid}/commit", async (Guid id, ImportService s, HttpContext ctx, CancellationToken ct)
            => (await s.CommitAsync(id, ctx.CurrentUserId(), ct)).ToHttpResult()).Produces<ImportBatchResponse>();
        g.MapPost("/imports/{id:guid}/cancel", async (Guid id, ImportService s, CancellationToken ct)
            => (await s.CancelAsync(id, ct)).ToHttpResult()).Produces<ImportBatchResponse>();
    }

    private static void MapReviewAndLifecycle(RouteGroupBuilder g)
    {
        g.MapGet("/review-queue", async (
            ReviewQueueService s, ContentKind? type, ContentOrigin? origin, bool? flagged, Guid? sourceId, int? page, int? pageSize, CancellationToken ct)
            => Results.Ok(await s.ListAsync(new ReviewQueueQuery(type, origin, sourceId), flagged, page ?? 1, Math.Clamp(pageSize ?? 20, 1, 100), ct)))
            .Produces<ReviewQueuePage>();

        g.MapPost("/{resource}/{id:guid}/submit-review", async (string resource, Guid id, LifecycleRequest req, LifecycleService s, CancellationToken ct)
            => (await s.SubmitReviewAsync(resource, id, req.Version, ct)).ToHttpResult()).Produces<ContentActionResponse>();
        g.MapPost("/{resource}/{id:guid}/return-to-draft", async (string resource, Guid id, LifecycleRequest req, LifecycleService s, CancellationToken ct)
            => (await s.ReturnToDraftAsync(resource, id, req.Version, req.Note, ct)).ToHttpResult()).Produces<ContentActionResponse>();
        g.MapPost("/{resource}/{id:guid}/archive", async (string resource, Guid id, LifecycleRequest req, LifecycleService s, CancellationToken ct)
            => (await s.ArchiveAsync(resource, id, req.Version, ct)).ToHttpResult()).Produces<ContentActionResponse>();
    }

    private static async Task<IFormCollection?> ReadFormAsync(HttpRequest req, CancellationToken ct)
        => req.HasFormContentType ? await req.ReadFormAsync(ct) : null;
}

public sealed record UploadResponse(MediaResponse Media, bool Reused);
