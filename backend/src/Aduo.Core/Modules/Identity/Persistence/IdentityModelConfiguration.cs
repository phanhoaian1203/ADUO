using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.Identity.Domain;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aduo.Core.Modules.Identity.Persistence;

// Mỗi class cấu hình một bảng của schema "identity". EF tự quét qua ApplyConfigurationsFromAssembly.

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.ToTable("users", Schemas.Identity, t =>
            t.HasCheckConstraint("ck_users_display_name_len", "char_length(display_name) BETWEEN 2 AND 50"));

        b.Property(u => u.DisplayName).HasMaxLength(50).IsRequired();
        b.Property(u => u.Email).HasMaxLength(254);
        b.Property(u => u.NormalizedEmail).HasMaxLength(254);

        // Email là định danh đăng nhập và phải duy nhất (BR-AUTH-01). Ghi đè chỉ mục mặc định (không unique) của Identity.
        b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("ix_users_normalized_email").IsUnique();

        // Phục vụ job ẩn danh: chỉ index các dòng đã bị đánh dấu xóa.
        b.HasIndex(u => u.DeletedAt).HasDatabaseName("ix_users_deleted_at").HasFilter("deleted_at IS NOT NULL");
    }
}

internal sealed class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> b)
    {
        b.ToTable("roles", Schemas.Identity);

        // Vai trò là dữ liệu cố định của hệ thống, tạo qua migration. GUID cố định để mọi môi trường giống nhau.
        b.HasData(
            new AppRole { Id = new Guid("0193a000-0000-7000-8000-000000000001"), Name = "Student", NormalizedName = "STUDENT", ConcurrencyStamp = "seed-student" },
            new AppRole { Id = new Guid("0193a000-0000-7000-8000-000000000002"), Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "seed-admin" });
    }
}

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> b) => b.ToTable("user_roles", Schemas.Identity);
}

internal sealed class UserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> b) => b.ToTable("user_claims", Schemas.Identity);
}

internal sealed class UserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> b) => b.ToTable("user_logins", Schemas.Identity);
}

internal sealed class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> b) => b.ToTable("role_claims", Schemas.Identity);
}

internal sealed class UserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> b) => b.ToTable("user_tokens", Schemas.Identity);
}

internal sealed class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    public void Configure(EntityTypeBuilder<DataProtectionKey> b) => b.ToTable("data_protection_keys", Schemas.Identity);
}

internal sealed class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> b)
    {
        b.ToTable("user_consents", Schemas.Identity);
        b.HasKey(c => c.Id);
        b.Property(c => c.TermsVersion).HasMaxLength(20).IsRequired();
        b.Property(c => c.PrivacyVersion).HasMaxLength(20).IsRequired();
        b.HasIndex(c => c.UserId).HasDatabaseName("ix_user_consents_user_id");
        b.HasOne<AppUser>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens", Schemas.Identity, t =>
            t.HasCheckConstraint("ck_refresh_tokens_expiry_order", "idle_expires_at <= absolute_expires_at"));

        b.HasKey(t => t.Id);
        b.Property(t => t.TokenHash).IsRequired();
        b.Property(t => t.IpPrefix).HasMaxLength(64);
        b.Property(t => t.UserAgent).HasMaxLength(200);
        b.Property(t => t.DeviceLabel).HasMaxLength(100);

        // Tra theo băm khi làm mới token. Phải duy nhất.
        b.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("ux_refresh_tokens_token_hash");
        b.HasIndex(t => t.FamilyId).HasDatabaseName("ix_refresh_tokens_family_id");
        // Liệt kê thiết bị đang đăng nhập: chỉ các token chưa thu hồi.
        b.HasIndex(t => t.UserId).HasDatabaseName("ix_refresh_tokens_user_active").HasFilter("revoked_at IS NULL");
        b.HasIndex(t => t.AbsoluteExpiresAt).HasDatabaseName("ix_refresh_tokens_absolute_expires_at");

        b.HasOne<AppUser>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class AuthEventConfiguration : IEntityTypeConfiguration<AuthEvent>
{
    public void Configure(EntityTypeBuilder<AuthEvent> b)
    {
        b.ToTable("auth_events", Schemas.Identity);
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();
        b.Property(e => e.IpPrefix).HasMaxLength(64);
        b.Property(e => e.UserAgent).HasMaxLength(200);
        b.Property(e => e.Details).HasColumnType("jsonb");

        b.HasIndex(e => new { e.UserId, e.OccurredAt }).IsDescending(false, true).HasDatabaseName("ix_auth_events_user_time");
        b.HasIndex(e => new { e.Kind, e.OccurredAt }).IsDescending(false, true).HasDatabaseName("ix_auth_events_kind_time");
    }
}
