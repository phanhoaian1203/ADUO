using System.Diagnostics;
using Serilog.Context;

namespace Aduo.Api.Middleware;

/// <summary>
/// Gắn traceId cho mỗi request (lấy từ Activity của ASP.NET Core, chuẩn W3C trace-context):
/// - trả về client qua header X-Trace-Id (người dùng báo lỗi kèm mã này là tìm được log ngay);
/// - đẩy vào LogContext để MỌI dòng log của request đều có traceId.
/// Phải đứng đầu pipeline.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Trace-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = traceId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("TraceId", traceId))
        {
            await next(context);
        }
    }
}
