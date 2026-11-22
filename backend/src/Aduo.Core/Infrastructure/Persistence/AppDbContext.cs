using Microsoft.EntityFrameworkCore;

namespace Aduo.Core.Infrastructure.Persistence;

/// <summary>
/// DbContext duy nhất của hệ thống (AR-03): một bộ migration, transaction đơn giản.
/// Mỗi module đăng ký cấu hình entity của mình qua IEntityTypeConfiguration trong thư mục Persistence của module.
/// ApplyConfigurationsFromAssembly tự quét toàn bộ các cấu hình đó.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Extension cần cho tìm kiếm (pg_trgm) và tìm không dấu (unaccent). Xem docs/phase-3/02 §1.
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.HasPostgresExtension("unaccent");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Enum lưu dạng text (đọc được khi xem DB, thêm giá trị mới không cần đổi số).
        // Mỗi entity vẫn khai báo CHECK constraint riêng trong cấu hình của nó.
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(40);
    }
}
