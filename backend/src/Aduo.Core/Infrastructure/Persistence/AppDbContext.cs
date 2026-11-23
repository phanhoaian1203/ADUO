using Aduo.Core.Modules.Identity.Domain;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aduo.Core.Infrastructure.Persistence;

/// <summary>
/// DbContext duy nhất của hệ thống (AR-03): một bộ migration, transaction đơn giản.
/// Kế thừa <see cref="IdentityDbContext{TUser,TRole,TKey}"/> để dùng kho người dùng của ASP.NET Core Identity,
/// và cài <see cref="IDataProtectionKeyContext"/> để lưu khóa Data Protection bền vững (deploy không làm hết hiệu lực
/// các link xác minh email/đặt lại mật khẩu đã gửi).
/// Mỗi module đăng ký cấu hình entity của mình qua IEntityTypeConfiguration trong thư mục Persistence của module.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<UserConsent> UserConsents => Set<UserConsent>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuthEvent> AuthEvents => Set<AuthEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Phải gọi base TRƯỚC: nó dựng các bảng chuẩn của Identity, rồi cấu hình của ta đổi tên bảng/schema.
        base.OnModelCreating(modelBuilder);

        // Extension cần cho tìm kiếm (pg_trgm) và tìm không dấu (unaccent). Xem docs/phase-3/02 §1.
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.HasPostgresExtension("unaccent");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Enum lưu dạng text (đọc được khi xem DB, thêm giá trị mới không cần đổi số).
        // Mỗi entity vẫn khai báo CHECK constraint riêng trong cấu hình của nó.
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(40);
    }
}
