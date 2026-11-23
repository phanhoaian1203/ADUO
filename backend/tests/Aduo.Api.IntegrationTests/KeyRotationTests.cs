using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace Aduo.Api.IntegrationTests;

/// <summary>ID-T40: xoay khóa ký JWT không làm gián đoạn người dùng (docs 01-identity §14.8).</summary>
[Collection(ApiCollection.Name)]
public class KeyRotationTests(ApiFactory factory)
{
    private const string OldKeyId = "test-1";
    private const string OldSecret = "test-only-jwt-key-0123456789-0123456789-abcdef";
    private const string NewKeyId = "test-2";
    private const string NewSecret = "rotated-jwt-key-0123456789-0123456789-abcdef-xyz";

    private async Task<HttpStatusCode> CallMeWithAsync(string accessToken, Action<IWebHostBuilder> configure)
    {
        using var rotated = factory.WithWebHostBuilder(configure);
        using var http = rotated.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/me");
        req.Headers.Add("Authorization", "Bearer " + accessToken);
        return (await http.SendAsync(req)).StatusCode;
    }

    [Fact]
    public async Task TokenSignedWithOldKey_StillValidWhileKeyIsListed_AndRejectedOnceRemoved()
    {
        var (client, _) = await TestUsers.SignedInAsync(factory); // token ký bằng khóa cũ (test-1)
        var token = client.Cookie("aduo_at")!;

        // Giai đoạn 1: khóa mới đứng đầu (dùng để ký), khóa cũ vẫn trong danh sách (dùng để xác minh).
        var overlap = await CallMeWithAsync(token, b =>
        {
            b.UseSetting("Jwt:Keys:0:KeyId", NewKeyId);
            b.UseSetting("Jwt:Keys:0:Secret", NewSecret);
            b.UseSetting("Jwt:Keys:1:KeyId", OldKeyId);
            b.UseSetting("Jwt:Keys:1:Secret", OldSecret);
        });
        Assert.Equal(HttpStatusCode.OK, overlap);

        // Giai đoạn 2: gỡ khóa cũ khỏi danh sách thì token ký bằng nó bị từ chối.
        var removed = await CallMeWithAsync(token, b =>
        {
            b.UseSetting("Jwt:Keys:0:KeyId", NewKeyId);
            b.UseSetting("Jwt:Keys:0:Secret", NewSecret);
        });
        Assert.Equal(HttpStatusCode.Unauthorized, removed);
    }
}
