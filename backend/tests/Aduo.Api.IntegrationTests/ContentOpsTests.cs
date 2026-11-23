using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Aduo.Core.Common.Content;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.ContentOps.Persistence;
using Aduo.Core.Modules.ContentOps.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aduo.Api.IntegrationTests;

/// <summary>Test case CO-T của docs/phase-4/modules/02-contentops.md (đợt 2a). Cần Docker (PostgreSQL thật).</summary>
[Collection(ApiCollection.Name)]
public class ContentOpsTests(ApiFactory factory)
{
    private const string Admin = "/api/v1/admin";
    private static readonly PublishChecklist FullChecklist = new(true, true, true, true, true, true);

    // ───────────── Nguồn, giấy phép, quyền ─────────────

    [Fact]
    public async Task Seed_ContainsLicensesAndSources()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);

        var licenses = await TestClient.JsonAsync(await admin.GetAsync($"{Admin}/licenses"));
        var sources = await TestClient.JsonAsync(await admin.GetAsync($"{Admin}/sources"));

        Assert.Contains(licenses.EnumerateArray(), l => l.GetProperty("code").GetString() == "CC-BY-SA-4.0");
        Assert.Contains(licenses.EnumerateArray(), l => l.GetProperty("code").GetString() == "CC-BY-NC-4.0" && !l.GetProperty("allowsCommercial").GetBoolean());
        Assert.Contains(sources.EnumerateArray(), s => s.GetProperty("name").GetString() == "Wiktionary (kaikki.org)");
        Assert.Contains(sources.EnumerateArray(), s => s.GetProperty("name").GetString() == "Azure TTS" && !s.TryGetProperty("licenseVerifiedAt", out _));
    }

    [Fact]
    public async Task AdminEndpoints_RejectStudentAndAnonymous() // CO-T20
    {
        var (student, _) = await TestUsers.SignedInAsync(factory);
        var anonymous = new TestClient(factory);

        foreach (var url in new[] { $"{Admin}/sources", $"{Admin}/media", $"{Admin}/imports", $"{Admin}/review-queue", $"{Admin}/settings" })
        {
            Assert.Equal(HttpStatusCode.Forbidden, (await student.GetAsync(url)).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(url)).StatusCode);
        }

        var publish = await student.PostAsync($"{Admin}/test-items/{Guid.NewGuid()}/publish", new { version = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, publish.StatusCode);
    }

    [Fact]
    public async Task Sources_CreateUpdate_ValidatesAndFlagsNonCommercial()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var name = "Nguồn thử " + Guid.NewGuid().ToString("N")[..8];

        var created = await admin.PostAsync($"{Admin}/sources", new { name, provider = "P", url = "https://example.org", defaultLicenseId = ContentSeed.LicCcByNc40 });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var json = await TestClient.JsonAsync(created);
        Assert.True(json.GetProperty("blocksPublishing").GetBoolean());

        var dup = await admin.PostAsync($"{Admin}/sources", new { name, defaultLicenseId = ContentSeed.LicCc0 });
        Assert.Equal("SOURCE_NAME_TAKEN", await TestClient.CodeAsync(dup));

        var badUrl = await admin.PostAsync($"{Admin}/sources", new { name = name + "x", url = "javascript:alert(1)", defaultLicenseId = ContentSeed.LicCc0 });
        Assert.Equal(HttpStatusCode.BadRequest, badUrl.StatusCode);

        var badLicense = await admin.PostAsync($"{Admin}/sources", new { name = name + "y", defaultLicenseId = 999 });
        Assert.Equal("LICENSE_NOT_FOUND", await TestClient.CodeAsync(badLicense));

        var id = json.GetProperty("id").GetString();
        var patched = await admin.SendAsync(HttpMethod.Patch, $"{Admin}/sources/{id}", new { defaultLicenseId = ContentSeed.LicCc0, licenseVerified = true });
        var after = await TestClient.JsonAsync(patched);
        Assert.False(after.GetProperty("blocksPublishing").GetBoolean());
        Assert.NotEqual(JsonValueKind.Null, after.GetProperty("licenseVerifiedAt").ValueKind);
    }

    // ───────────── Media ─────────────

    private static MultipartFormDataContent Upload(byte[] bytes, string fileName, Guid sourceId, short? licenseId = null)
    {
        var form = new MultipartFormDataContent
        {
            { ByteContent(bytes), "file", fileName },
            { new StringContent(sourceId.ToString()), "sourceId" },
        };
        if (licenseId is not null)
        {
            form.Add(new StringContent(licenseId.ToString()!), "licenseId");
        }

        return form;
    }

    private static ByteArrayContent ByteContent(byte[] bytes)
    {
        var c = new ByteArrayContent(bytes);
        c.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        return c;
    }

    private static byte[] Mp3(int totalBytes, byte fill = 0)
    {
        var data = new byte[totalBytes];
        Array.Fill(data, fill);
        "ID3\u0004\0\0\0\0\0\0"u8.CopyTo(data);
        return data;
    }

    [Fact]
    public async Task UploadAudio_Succeeds_AndCanBeDownloaded()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var bytes = Mp3(2048, fill: 7);

        var res = await admin.SendAsync(HttpMethod.Post, $"{Admin}/media", Upload(bytes, "mutter.mp3", ContentSeed.SrcAduo));

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var media = (await TestClient.JsonAsync(res)).GetProperty("media");
        Assert.Equal("audio/mpeg", media.GetProperty("contentType").GetString());
        Assert.Equal("Audio", media.GetProperty("kind").GetString());

        var file = await admin.GetAsync($"{Admin}/media/{media.GetProperty("id").GetString()}/file");
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        var http = factory.CreateClient();
        _ = http;
    }

    [Fact]
    public async Task UploadExecutableNamedMp3_IsRejected() // CO-T11
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var exe = new byte[1024];
        exe[0] = 0x4D;
        exe[1] = 0x5A;

        var res = await admin.SendAsync(HttpMethod.Post, $"{Admin}/media", Upload(exe, "song.mp3", ContentSeed.SrcAduo));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("INVALID_FILE_TYPE", await TestClient.CodeAsync(res));
    }

    [Fact]
    public async Task UploadAudioOver5MB_IsRejected() // CO-T12
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);

        var res = await admin.SendAsync(HttpMethod.Post, $"{Admin}/media", Upload(Mp3(6 * 1024 * 1024), "long.mp3", ContentSeed.SrcAduo));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("FILE_TOO_LARGE", await TestClient.CodeAsync(res));
    }

    [Fact]
    public async Task UploadSameContentTwice_ReturnsExistingMedia() // CO-T13
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var bytes = Mp3(4096, fill: (byte)Random.Shared.Next(1, 250));

        var first = await admin.SendAsync(HttpMethod.Post, $"{Admin}/media", Upload(bytes, "a.mp3", ContentSeed.SrcAduo));
        var second = await admin.SendAsync(HttpMethod.Post, $"{Admin}/media", Upload(bytes, "copy-of-a.mp3", ContentSeed.SrcAduo));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var a = await TestClient.JsonAsync(first);
        var b = await TestClient.JsonAsync(second);
        Assert.True(b.GetProperty("reused").GetBoolean());
        Assert.Equal(a.GetProperty("media").GetProperty("id").GetString(), b.GetProperty("media").GetProperty("id").GetString());
    }

    [Fact]
    public async Task Upload_FromNonCommercialSource_IsRejected()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);

        var res = await admin.SendAsync(HttpMethod.Post, $"{Admin}/media", Upload(Mp3(1500, fill: 9), "nc.mp3", ContentSeed.SrcAduo, ContentSeed.LicCcByNc40));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        Assert.Equal("LICENSE_NOT_COMMERCIAL", await TestClient.CodeAsync(res));
    }

    [Fact]
    public async Task Upload_StoredNameIsUuid_NotTheClientFileName()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var res = await admin.SendAsync(HttpMethod.Post, $"{Admin}/media", Upload(Mp3(1100, fill: 11), "../../etc/passwd.mp3", ContentSeed.SrcAduo));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var id = Guid.Parse((await TestClient.JsonAsync(res)).GetProperty("media").GetProperty("id").GetString()!);

        await using var scope = factory.Services.CreateAsyncScope();
        var m = await scope.ServiceProvider.GetRequiredService<AppDbContext>().MediaAssets.SingleAsync(x => x.Id == id);

        Assert.DoesNotContain("passwd", m.StorageKey);
        Assert.DoesNotContain("..", m.StorageKey);
        Assert.Matches(@"^audio/\d{4}/[0-9a-f]{32}\.mp3$", m.StorageKey);
        Assert.Equal("passwd.mp3", m.OriginalFileName);
    }

    // ───────────── Import ─────────────

    private static string Csv(string prefix, int total, int invalid, IEnumerable<int>? skip = null)
    {
        var sb = new StringBuilder("key,name\n");
        for (var i = 1; i <= total; i++)
        {
            sb.Append(prefix).Append(i).Append(',').Append(i <= invalid ? "" : "Wort" + i).Append('\n');
        }

        return sb.ToString();
    }

    private static MultipartFormDataContent ImportForm(string csv, Guid sourceId, string contentType = "test-rows")
        => new()
        {
            { ByteContent(Encoding.UTF8.GetBytes(csv)), "file", "words.csv" },
            { new StringContent(sourceId.ToString()), "sourceId" },
            { new StringContent(contentType), "contentType" },
        };

    [Fact]
    public async Task Import_FromNonCommercialSource_Returns422AndCreatesNoBatch() // CO-T01
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var created = await admin.PostAsync($"{Admin}/sources", new { name = "NC " + Guid.NewGuid().ToString("N")[..8], defaultLicenseId = ContentSeed.LicCcByNc40 });
        var ncId = Guid.Parse((await TestClient.JsonAsync(created)).GetProperty("id").GetString()!);
        var before = await BatchCountAsync();

        var res = await admin.SendAsync(HttpMethod.Post, $"{Admin}/imports", ImportForm(Csv("nc", 5, 0), ncId));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        Assert.Equal("LICENSE_NOT_COMMERCIAL", await TestClient.CodeAsync(res));
        Assert.Equal(before, await BatchCountAsync());
    }

    [Fact]
    public async Task Import_UnknownContentType_Returns400()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);

        var res = await admin.SendAsync(HttpMethod.Post, $"{Admin}/imports", ImportForm(Csv("u", 2, 0), ContentSeed.SrcAduo, "nope"));

        Assert.Equal("UNKNOWN_CONTENT_TYPE", await TestClient.CodeAsync(res));
    }

    [Fact]
    public async Task Import_DryRunCommitRepeatAndCancel_FollowTheDesign() // CO-T02, T03, T04, T05
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var p = "k" + Guid.NewGuid().ToString("N")[..6] + "_";

        // 500 dòng: 15 dòng lỗi (thiếu nghĩa), 5 dòng đã có trong "DB", 480 dòng mới hợp lệ.
        for (var i = 16; i <= 20; i++)
        {
            FakeImportHandler.Store["de|" + p + i] = new FakeStored(p + i, "đã có", Guid.NewGuid(), ContentSeed.SrcAduo, 1, true);
        }

        var csv = Csv(p, 500, invalid: 15);

        // CO-T02: dry-run trả báo cáo đúng số liệu và KHÔNG tạo bản ghi nội dung.
        var dry = await admin.SendAsync(HttpMethod.Post, $"{Admin}/imports", ImportForm(csv, ContentSeed.SrcWiktionary));
        Assert.Equal(HttpStatusCode.Created, dry.StatusCode);
        var batch = await TestClient.JsonAsync(dry);
        var report = batch.GetProperty("report");
        Assert.Equal(500, report.GetProperty("total").GetInt32());
        Assert.Equal(480, report.GetProperty("valid").GetInt32());
        Assert.Equal(15, report.GetProperty("errors").GetArrayLength());
        Assert.Equal(5, report.GetProperty("duplicates").GetArrayLength());
        Assert.Equal(5, FakeImportHandler.Store.Count(kv => kv.Key.StartsWith("de|" + p, StringComparison.Ordinal)));
        var batchId = batch.GetProperty("id").GetString();

        // CO-T03: commit tạo 480 bản ghi, mỗi bản ghi mang provenance (batch, nguồn, giấy phép).
        var commit = await admin.PostAsync($"{Admin}/imports/{batchId}/commit");
        Assert.Equal(HttpStatusCode.OK, commit.StatusCode);
        Assert.Equal(480, (await TestClient.JsonAsync(commit)).GetProperty("report").GetProperty("inserted").GetInt32());
        var mine = FakeImportHandler.Store.Where(kv => kv.Value.BatchId == Guid.Parse(batchId!)).ToList();
        Assert.Equal(480, mine.Count);
        Assert.All(mine, kv =>
        {
            Assert.Equal(ContentSeed.SrcWiktionary, kv.Value.SourceId);
            Assert.Equal(ContentSeed.LicCcBySa40, kv.Value.LicenseId);
        });

        // Commit lần hai cùng batch: bị từ chối, không tạo thêm.
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsync($"{Admin}/imports/{batchId}/commit")).StatusCode);

        // CO-T04: import lại cùng file: không còn dòng mới, 485 dòng thành "trùng".
        var again = await TestClient.JsonAsync(await admin.SendAsync(HttpMethod.Post, $"{Admin}/imports", ImportForm(csv, ContentSeed.SrcWiktionary)));
        Assert.Equal(0, again.GetProperty("report").GetProperty("valid").GetInt32());
        Assert.Equal(485, again.GetProperty("report").GetProperty("duplicates").GetArrayLength());
        var secondCommit = await TestClient.JsonAsync(await admin.PostAsync($"{Admin}/imports/{again.GetProperty("id").GetString()}/commit"));
        Assert.Equal(0, secondCommit.GetProperty("report").GetProperty("inserted").GetInt32());

        // CO-T05: hủy batch có 10 bản ghi đã Published: xóa 470, giữ 10.
        foreach (var kv in mine.Take(10))
        {
            FakeImportHandler.Store[kv.Key] = kv.Value with { Published = true };
        }

        var cancel = await admin.PostAsync($"{Admin}/imports/{batchId}/cancel");
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);
        var cancelled = (await TestClient.JsonAsync(cancel)).GetProperty("report");
        Assert.Equal(470, cancelled.GetProperty("deleted").GetInt32());
        Assert.Equal(10, cancelled.GetProperty("keptPublished").GetInt32());
        Assert.Equal(10, FakeImportHandler.Store.Count(kv => kv.Value.BatchId == Guid.Parse(batchId!)));

        Assert.Equal("BATCH_ALREADY_CANCELLED", await TestClient.CodeAsync(await admin.PostAsync($"{Admin}/imports/{batchId}/cancel")));
    }

    [Fact]
    public async Task Import_ParallelCommits_OnlyOneWins()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var (admin2, _) = await TestAdmins.SignedInAdminAsync(factory);
        var p = "p" + Guid.NewGuid().ToString("N")[..6] + "_";
        var dry = await TestClient.JsonAsync(await admin.SendAsync(HttpMethod.Post, $"{Admin}/imports", ImportForm(Csv(p, 50, 0), ContentSeed.SrcAduo)));
        var id = dry.GetProperty("id").GetString();

        var results = await Task.WhenAll(admin.PostAsync($"{Admin}/imports/{id}/commit"), admin2.PostAsync($"{Admin}/imports/{id}/commit"));

        Assert.Equal(1, results.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(1, results.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        Assert.Equal(50, FakeImportHandler.Store.Count(kv => kv.Key.StartsWith("de|" + p, StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Import_Template_IsDownloadable()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);

        var res = await admin.GetAsync($"{Admin}/imports/templates/test-rows");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.StartsWith("key,name", await res.Content.ReadAsStringAsync());
    }

    // ───────────── Vòng đời qua API (loại nội dung giả) ─────────────

    private static object Body(int version, ChecklistRequest? checklist = null, string? note = null)
        => new { version, checklist, note };

    private static readonly ChecklistRequest Full = new(true, true, true, true, true, true);

    [Fact]
    public async Task Publish_WithoutChecklist_Returns422WithReasons() // CO-T06
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var item = factory.Content.Add(ContentSeed.SrcAduo, ContentSeed.LicProprietary);

        var res = await admin.PostAsync($"{Admin}/test-items/{item.Id}/publish", Body(item.Version));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        Assert.Equal("PUBLISH_CHECK_FAILED", await TestClient.CodeAsync(res));
        var reasons = (await TestClient.JsonAsync(res)).GetProperty("errors").GetProperty("reasons").EnumerateArray().Select(r => r.GetString()).ToList();
        Assert.Contains("CHECKLIST_INCOMPLETE", reasons);
        Assert.Equal(ContentStatus.Draft, item.Meta.Status);
    }

    [Fact]
    public async Task Publish_AiDraftWithoutReview_IsRejected_ThenPassesAfterReview() // CO-T07
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var item = factory.Content.Add(ContentSeed.SrcAduo, ContentSeed.LicProprietary, origin: ContentOrigin.AIDraft);

        var res = await admin.PostAsync($"{Admin}/test-items/{item.Id}/publish", Body(item.Version, Full));
        var reasons = (await TestClient.JsonAsync(res)).GetProperty("errors").GetProperty("reasons").EnumerateArray().Select(r => r.GetString()).ToList();
        Assert.Contains("AI_REVIEW_MISSING", reasons);

        item.AiReviewed = true;
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"{Admin}/test-items/{item.Id}/publish", Body(item.Version, Full))).StatusCode);
    }

    [Fact]
    public async Task Publish_Succeeds_AndRecordsChecklistAndRevisionAndReviewer()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var item = factory.Content.Add(ContentSeed.SrcWiktionary, ContentSeed.LicCcBySa40, "die Mutter");

        var res = await admin.PostAsync($"{Admin}/test-items/{item.Id}/publish", Body(item.Version, Full));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(ContentStatus.Published, item.Meta.Status);
        Assert.NotNull(item.Meta.ReviewedBy);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await db.ReviewChecklists.CountAsync(c => c.EntityId == item.Id));
        Assert.Equal(1, await db.ContentRevisions.CountAsync(r => r.EntityId == item.Id));
    }

    [Fact]
    public async Task Publish_FromNonCommercialLicense_IsBlocked() // BR-CNT-03
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var item = factory.Content.Add(ContentSeed.SrcAduo, ContentSeed.LicCcByNc40);

        var res = await admin.PostAsync($"{Admin}/test-items/{item.Id}/publish", Body(item.Version, Full));

        var reasons = (await TestClient.JsonAsync(res)).GetProperty("errors").GetProperty("reasons").EnumerateArray().Select(r => r.GetString()).ToList();
        Assert.Contains("LICENSE_NOT_COMMERCIAL", reasons);
    }

    [Fact]
    public async Task ReturnToDraft_RequiresNote() // CO-T08
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var item = factory.Content.Add(ContentSeed.SrcAduo, ContentSeed.LicProprietary);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"{Admin}/test-items/{item.Id}/submit-review", Body(item.Version))).StatusCode);

        var without = await admin.PostAsync($"{Admin}/test-items/{item.Id}/return-to-draft", Body(item.Version));
        var with = await admin.PostAsync($"{Admin}/test-items/{item.Id}/return-to-draft", Body(item.Version, note: "Sửa lại nghĩa"));

        Assert.Equal(HttpStatusCode.BadRequest, without.StatusCode);
        Assert.Equal(HttpStatusCode.OK, with.StatusCode);
        Assert.Equal(ContentStatus.Draft, item.Meta.Status);
    }

    [Fact]
    public async Task StaleVersion_Returns409() // CO-T09
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var item = factory.Content.Add(ContentSeed.SrcAduo, ContentSeed.LicProprietary);
        item.Version = 3; // có người đã sửa nội dung này sau khi tab của bạn mở ra (bạn đang thấy version 2)

        var res = await admin.PostAsync($"{Admin}/test-items/{item.Id}/submit-review", Body(version: 2));

        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        Assert.Equal("CONCURRENCY_CONFLICT", await TestClient.CodeAsync(res));
        Assert.Equal(ContentStatus.Draft, item.Meta.Status);
    }

    [Fact]
    public async Task InvalidTransition_Returns422_UnknownResourceAndItem_Return404()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var item = factory.Content.Add(ContentSeed.SrcAduo, ContentSeed.LicProprietary);

        var archive = await admin.PostAsync($"{Admin}/test-items/{item.Id}/archive", Body(item.Version));
        var unknown = await admin.PostAsync($"{Admin}/khong-co/{item.Id}/submit-review", Body(1));
        var missing = await admin.PostAsync($"{Admin}/test-items/{Guid.NewGuid()}/submit-review", Body(1));

        Assert.Equal("INVALID_TRANSITION", await TestClient.CodeAsync(archive));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task EditingPublishedContent_IncrementsVersionAndStoresOneRevision() // CO-T10
    {
        var item = factory.Content.Add(ContentSeed.SrcAduo, ContentSeed.LicProprietary, "die Mutter");
        Assert.True(Aduo.Core.Common.Content.ContentLifecycle.Publish(item,
            new PublishContext(Guid.NewGuid(), DateTimeOffset.UtcNow, true, true, FullChecklist)).IsSuccess);

        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IContentVersioning>()
            .RecordEditAsync("test-items", item, Guid.NewGuid(), "Sửa ví dụ", CancellationToken.None);
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().SaveChangesAsync();

        Assert.Equal(2, item.Version);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var revisions = await db.ContentRevisions.Where(r => r.EntityId == item.Id).ToListAsync();
        Assert.Single(revisions);
        Assert.Equal(1, revisions[0].Version);
        Assert.Contains("die Mutter", revisions[0].Snapshot);
    }

    [Fact]
    public async Task EditingDraft_DoesNotCreateRevision()
    {
        var item = factory.Content.Add(ContentSeed.SrcAduo, ContentSeed.LicProprietary);

        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IContentVersioning>()
            .RecordEditAsync("test-items", item, Guid.NewGuid(), null, CancellationToken.None);
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().SaveChangesAsync();

        Assert.Equal(1, item.Version);
        Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AppDbContext>().ContentRevisions.CountAsync(r => r.EntityId == item.Id));
    }

    // ───────────── Hàng đợi duyệt, hàng loạt, ghi công ─────────────

    [Fact]
    public async Task ReviewQueue_FlaggedFirst_AndBatchPublishSkipsFlagged()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var sourceId = (await TestClient.JsonAsync(await admin.PostAsync($"{Admin}/sources",
            new { name = "Hàng đợi " + Guid.NewGuid().ToString("N")[..8], defaultLicenseId = ContentSeed.LicCc0 })))
            .GetProperty("id").GetString()!;
        var src = Guid.Parse(sourceId);
        var clean1 = factory.Content.Add(src, ContentSeed.LicCc0, "sạch 1");
        var clean2 = factory.Content.Add(src, ContentSeed.LicCc0, "sạch 2");
        var flagged = factory.Content.Add(src, ContentSeed.LicCc0, "có cờ");
        flagged.FlagList.Add("AUTO_CHECK_FAILED");

        var queue = await TestClient.JsonAsync(await admin.GetAsync($"{Admin}/review-queue?sourceId={sourceId}"));
        var titles = queue.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("title").GetString()).ToList();
        Assert.Equal("có cờ", titles[0]); // mục có cờ lên đầu
        Assert.Equal(3, queue.GetProperty("total").GetInt32());

        var batch = await admin.PostAsync($"{Admin}/review-queue/publish-batch", new
        {
            items = new[]
            {
                new { resource = "test-items", id = clean1.Id, version = 1 },
                new { resource = "test-items", id = clean2.Id, version = 1 },
                new { resource = "test-items", id = flagged.Id, version = 1 },
            },
            checklist = Full,
        });

        Assert.Equal(HttpStatusCode.OK, batch.StatusCode);
        Assert.Equal(ContentStatus.Published, clean1.Meta.Status);
        Assert.Equal(ContentStatus.Published, clean2.Meta.Status);
        Assert.Equal(ContentStatus.Draft, flagged.Meta.Status);
        var outcomes = (await TestClient.JsonAsync(batch)).EnumerateArray().ToList();
        Assert.Equal("FLAGGED", outcomes.Single(o => o.GetProperty("id").GetString() == flagged.Id.ToString()).GetProperty("code").GetString());

        // Mỗi mục trong lô đều có bằng chứng checklist riêng.
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(2, await db.ReviewChecklists.CountAsync(c => c.EntityId == clean1.Id || c.EntityId == clean2.Id));
    }

    [Fact]
    public async Task Attributions_ListsOnlySourcesWithPublishedContent() // CO-T19
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);
        var published = Guid.Parse((await TestClient.JsonAsync(await admin.PostAsync($"{Admin}/sources",
            new { name = "Có nội dung " + Guid.NewGuid().ToString("N")[..8], defaultLicenseId = ContentSeed.LicCcBySa40 }))).GetProperty("id").GetString()!);
        var onlyDraft = Guid.Parse((await TestClient.JsonAsync(await admin.PostAsync($"{Admin}/sources",
            new { name = "Chỉ nháp " + Guid.NewGuid().ToString("N")[..8], defaultLicenseId = ContentSeed.LicCcBySa40 }))).GetProperty("id").GetString()!);
        var item = factory.Content.Add(published, ContentSeed.LicCcBySa40);
        factory.Content.Add(onlyDraft, ContentSeed.LicCcBySa40);
        await admin.PostAsync($"{Admin}/test-items/{item.Id}/publish", Body(item.Version, Full));

        var anonymous = new TestClient(factory);
        var res = await anonymous.GetAsync("/api/v1/attributions"); // công khai, không cần đăng nhập

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var list = (await TestClient.JsonAsync(res)).EnumerateArray().ToList();
        Assert.Contains(list, a => a.GetProperty("sourceId").GetString() == published.ToString()
                                   && a.GetProperty("licenseCode").GetString() == "CC-BY-SA-4.0"
                                   && a.GetProperty("shareAlike").GetBoolean());
        Assert.DoesNotContain(list, a => a.GetProperty("sourceId").GetString() == onlyDraft.ToString());
    }

    // ───────────── Cài đặt ─────────────

    [Fact]
    public async Task Settings_DefaultsAreSeeded_UpdateIsValidatedAndPersisted()
    {
        var (admin, _) = await TestAdmins.SignedInAdminAsync(factory);

        var list = (await TestClient.JsonAsync(await admin.GetAsync($"{Admin}/settings"))).EnumerateArray().ToList();
        Assert.Equal(60, list.Single(s => s.GetProperty("key").GetString() == "lesson.passThresholdPct").GetProperty("value").GetDecimal());
        Assert.Equal(10, list.Single(s => s.GetProperty("key").GetString() == "budget.monthlyUsd").GetProperty("value").GetDecimal());

        var tooHigh = await admin.SendAsync(HttpMethod.Patch, $"{Admin}/settings", new { values = new Dictionary<string, decimal> { ["lesson.passThresholdPct"] = 150 } });
        var unknown = await admin.SendAsync(HttpMethod.Patch, $"{Admin}/settings", new { values = new Dictionary<string, decimal> { ["khong.ton.tai"] = 1 } });
        Assert.Equal(HttpStatusCode.BadRequest, tooHigh.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);

        var ok = await admin.SendAsync(HttpMethod.Patch, $"{Admin}/settings", new { values = new Dictionary<string, decimal> { ["lesson.passThresholdPct"] = 70 } });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        Assert.Equal(70, await scope.ServiceProvider.GetRequiredService<Aduo.Core.Common.Settings.ISettings>()
            .GetNumberAsync("lesson.passThresholdPct", CancellationToken.None));

        // Trả về mặc định để không ảnh hưởng test khác.
        await admin.SendAsync(HttpMethod.Patch, $"{Admin}/settings", new { values = new Dictionary<string, decimal> { ["lesson.passThresholdPct"] = 60 } });
    }

    private async Task<int> BatchCountAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().ImportBatches.CountAsync();
    }
}
