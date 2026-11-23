using Aduo.Core;
using Aduo.Core.Common.Security;
using Aduo.Core.Modules.Identity;
using Aduo.Core.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aduo.Cli;

/// <summary>Tạo tài khoản Admin đầu tiên (docs 01-identity §7). Không có API công khai nào tạo Admin.</summary>
internal static class CreateAdminCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        var email = Option(args, "--email");
        var name = Option(args, "--name") ?? "Quản trị viên";
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Error.WriteLine("Thiếu --email. Ví dụ: aduo-cli create-admin --email admin@example.com");
            return 1;
        }

        var password = Environment.GetEnvironmentVariable("ADUO_ADMIN_PASSWORD") ?? ReadPassword();
        if (string.IsNullOrEmpty(password))
        {
            Console.Error.WriteLine("Mật khẩu trống.");
            return 1;
        }

        var builder = Host.CreateApplicationBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        if (builder.Environment.IsDevelopment() && string.IsNullOrEmpty(builder.Configuration.GetConnectionString("Aduo")))
        {
            // Chỉ dành cho máy dev (khớp deploy/compose.dev.yml). Production phải đặt ConnectionStrings__Aduo.
            builder.Configuration["ConnectionStrings:Aduo"] = "Host=localhost;Port=5432;Database=aduo_dev;Username=aduo;Password=aduo_dev_only";
        }

        builder.Services.AddAduoCore(builder.Configuration);
        builder.Services.AddIdentityUserStore();
        using var host = builder.Build();
        await using var scope = host.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser { UserName = email, Email = email, EmailConfirmed = true, DisplayName = name, CreatedAt = DateTimeOffset.UtcNow };
            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                Console.Error.WriteLine("Không tạo được tài khoản: " + string.Join("; ", created.Errors.Select(e => e.Description)));
                return 1;
            }
        }

        if (!await users.IsInRoleAsync(user, Roles.Admin))
        {
            await users.AddToRoleAsync(user, Roles.Admin);
        }

        Console.WriteLine($"Đã tạo/cập nhật Admin: {email}");
        return 0;
    }

    private static string? Option(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    /// <summary>Đọc mật khẩu không hiện ký tự trên màn hình.</summary>
    private static string ReadPassword()
    {
        Console.Write("Mật khẩu Admin: ");
        var chars = new List<char>();
        while (Console.ReadKey(intercept: true) is var key && key.Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (chars.Count > 0)
                {
                    chars.RemoveAt(chars.Count - 1);
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                chars.Add(key.KeyChar);
            }
        }

        Console.WriteLine();
        return new string(chars.ToArray());
    }
}
