using Aduo.Core.Common.Content;
using Aduo.Core.Infrastructure.Content;
using Aduo.Core.Infrastructure.Persistence;
using Aduo.Core.Modules.ContentOps.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aduo.Core.Modules.ContentOps.Persistence;

/// <summary>Ids cố định của dữ liệu seed, để mọi môi trường (dev, test, production) giống hệt nhau.</summary>
public static class ContentSeed
{
    public const short LicCcBySa40 = 1;
    public const short LicCcBy20Fr = 2;
    public const short LicCc0 = 3;
    public const short LicProprietary = 4;
    public const short LicCcByNc40 = 5;

    public static readonly Guid SrcWiktionary = new("0193b000-0000-7000-8000-000000000001");
    public static readonly Guid SrcTatoeba = new("0193b000-0000-7000-8000-000000000002");
    public static readonly Guid SrcAduo = new("0193b000-0000-7000-8000-000000000003");
    public static readonly Guid SrcAzureTts = new("0193b000-0000-7000-8000-000000000004");

    public static readonly DateTimeOffset SeedTime = new(2026, 11, 23, 0, 0, 0, TimeSpan.Zero);
}

internal sealed class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> b)
    {
        b.ToTable("licenses", Schemas.Content);
        b.HasKey(l => l.Id);
        b.Property(l => l.Id).ValueGeneratedNever();
        b.Property(l => l.Code).HasMaxLength(40).IsRequired();
        b.Property(l => l.Name).HasMaxLength(120).IsRequired();
        b.Property(l => l.Url).HasMaxLength(300);
        b.HasIndex(l => l.Code).IsUnique().HasDatabaseName("ux_licenses_code");

        b.HasData(
            new License { Id = ContentSeed.LicCcBySa40, Code = "CC-BY-SA-4.0", Name = "Creative Commons Attribution-ShareAlike 4.0", Url = "https://creativecommons.org/licenses/by-sa/4.0/", AllowsCommercial = true, RequiresAttribution = true, ShareAlike = true, AllowsModification = true },
            new License { Id = ContentSeed.LicCcBy20Fr, Code = "CC-BY-2.0-FR", Name = "Creative Commons Attribution 2.0 France", Url = "https://creativecommons.org/licenses/by/2.0/fr/", AllowsCommercial = true, RequiresAttribution = true, ShareAlike = false, AllowsModification = true },
            new License { Id = ContentSeed.LicCc0, Code = "CC0-1.0", Name = "Creative Commons Zero 1.0 (public domain)", Url = "https://creativecommons.org/publicdomain/zero/1.0/", AllowsCommercial = true, RequiresAttribution = false, ShareAlike = false, AllowsModification = true },
            new License { Id = ContentSeed.LicProprietary, Code = "Proprietary", Name = "Độc quyền (nội dung của ADUO hoặc theo điều khoản nhà cung cấp)", Url = null, AllowsCommercial = true, RequiresAttribution = false, ShareAlike = false, AllowsModification = true },
            // Có sẵn để minh họa và kiểm thử cơ chế chặn: nguồn gắn license này không import/publish được (BR-CNT-03).
            new License { Id = ContentSeed.LicCcByNc40, Code = "CC-BY-NC-4.0", Name = "Creative Commons Attribution-NonCommercial 4.0 (KHÔNG dùng thương mại)", Url = "https://creativecommons.org/licenses/by-nc/4.0/", AllowsCommercial = false, RequiresAttribution = true, ShareAlike = false, AllowsModification = true });
    }
}

internal sealed class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> b)
    {
        b.ToTable("sources", Schemas.Content);
        b.HasKey(s => s.Id);
        b.Property(s => s.Name).HasMaxLength(120).IsRequired();
        b.Property(s => s.Provider).HasMaxLength(120);
        b.Property(s => s.Url).HasMaxLength(300);
        b.Property(s => s.Notes).HasMaxLength(1000);
        b.HasIndex(s => s.Name).IsUnique().HasDatabaseName("ux_sources_name");
        b.HasOne<License>().WithMany().HasForeignKey(s => s.DefaultLicenseId).OnDelete(DeleteBehavior.Restrict);

        // LicenseVerifiedAt để null: chưa ai đọc lại giấy phép ở trang gốc, đúng sự thật (⚖️ NEEDS LEGAL VERIFICATION).
        b.HasData(
            new Source { Id = ContentSeed.SrcWiktionary, Name = "Wiktionary (kaikki.org)", Provider = "Wiktionary / kaikki.org", Url = "https://kaikki.org", DefaultLicenseId = ContentSeed.LicCcBySa40, IsActive = true, Notes = "Dữ liệu CC BY-SA: bản dịch và biến đổi vẫn là CC BY-SA, phải ghi công trên UI (BR-CNT-04).", CreatedAt = ContentSeed.SeedTime, UpdatedAt = ContentSeed.SeedTime },
            new Source { Id = ContentSeed.SrcTatoeba, Name = "Tatoeba", Provider = "Tatoeba", Url = "https://tatoeba.org", DefaultLicenseId = ContentSeed.LicCcBy20Fr, IsActive = true, Notes = "Phần lớn câu CC BY 2.0 FR; một số câu CC0. Kiểm tra từng câu khi import.", CreatedAt = ContentSeed.SeedTime, UpdatedAt = ContentSeed.SeedTime },
            new Source { Id = ContentSeed.SrcAduo, Name = "ADUO (tự biên soạn)", Provider = "ADUO", DefaultLicenseId = ContentSeed.LicProprietary, IsActive = true, CreatedAt = ContentSeed.SeedTime, UpdatedAt = ContentSeed.SeedTime },
            new Source { Id = ContentSeed.SrcAzureTts, Name = "Azure TTS", Provider = "Microsoft Azure", DefaultLicenseId = ContentSeed.LicProprietary, IsActive = true, Notes = "⚖️ Chưa xác minh điều khoản lưu và phát lại audio tổng hợp. Xác minh trước khi dùng audio thật.", CreatedAt = ContentSeed.SeedTime, UpdatedAt = ContentSeed.SeedTime });
    }
}

internal sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> b)
    {
        b.ToTable("media", Schemas.Content);
        b.HasKey(m => m.Id);
        b.Property(m => m.Bucket).HasMaxLength(60).IsRequired();
        b.Property(m => m.StorageKey).HasMaxLength(300).IsRequired();
        b.Property(m => m.ContentType).HasMaxLength(100).IsRequired();
        b.Property(m => m.OriginalFileName).HasMaxLength(200);
        b.Property(m => m.ChecksumSha256).HasMaxLength(64).IsRequired();
        b.Property(m => m.OriginalUrl).HasMaxLength(500);
        b.Property(m => m.Attribution).HasMaxLength(500);
        b.HasIndex(m => m.StorageKey).IsUnique().HasDatabaseName("ux_media_storage_key");
        b.HasIndex(m => m.ChecksumSha256).HasDatabaseName("ix_media_checksum");
        b.HasIndex(m => new { m.Kind, m.CreatedAt }).IsDescending(false, true).HasDatabaseName("ix_media_kind_created");
        b.ConfigureContentMeta(m => m.Meta);
    }
}

internal sealed class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> b)
    {
        b.ToTable("import_batches", Schemas.Content);
        b.HasKey(i => i.Id);
        b.Property(i => i.ContentType).HasMaxLength(60).IsRequired();
        b.Property(i => i.Report).HasColumnType("jsonb").IsRequired();
        b.HasIndex(i => i.CreatedAt).IsDescending().HasDatabaseName("ix_import_batches_created");
        b.HasOne<Source>().WithMany().HasForeignKey(i => i.SourceId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<MediaAsset>().WithMany().HasForeignKey(i => i.FileMediaId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ContentRevisionConfiguration : IEntityTypeConfiguration<ContentRevision>
{
    public void Configure(EntityTypeBuilder<ContentRevision> b)
    {
        b.ToTable("content_revisions", Schemas.Content);
        b.HasKey(r => r.Id);
        b.Property(r => r.EntityType).HasMaxLength(60).IsRequired();
        b.Property(r => r.Snapshot).HasColumnType("jsonb").IsRequired();
        b.Property(r => r.ChangeNote).HasMaxLength(500);
        b.HasIndex(r => new { r.EntityType, r.EntityId, r.Version }).IsDescending(false, false, true).HasDatabaseName("ix_content_revisions_entity");
    }
}

internal sealed class ReviewChecklistConfiguration : IEntityTypeConfiguration<ReviewChecklistRecord>
{
    public void Configure(EntityTypeBuilder<ReviewChecklistRecord> b)
    {
        b.ToTable("review_checklists", Schemas.Content);
        b.HasKey(r => r.Id);
        b.Property(r => r.EntityType).HasMaxLength(60).IsRequired();
        b.Property(r => r.Items).HasColumnType("jsonb").IsRequired();
        b.HasIndex(r => new { r.EntityType, r.EntityId }).HasDatabaseName("ix_review_checklists_entity");
    }
}

internal sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> b)
    {
        b.ToTable("app_settings", Schemas.Content);
        b.HasKey(s => s.Key);
        b.Property(s => s.Key).HasMaxLength(100);
        b.Property(s => s.Value).HasColumnType("jsonb").IsRequired();

        b.HasData(SettingDefinitions.All.Select(d =>
            new AppSetting { Key = d.Key, Value = d.Default.ToString(System.Globalization.CultureInfo.InvariantCulture), UpdatedAt = ContentSeed.SeedTime }));
    }
}

/// <summary>Các giá trị cấu hình hợp lệ của business rules (docs 02-contentops §4.6) cùng giá trị mặc định và khoảng cho phép.</summary>
public sealed record SettingDefinition(string Key, decimal Default, decimal Min, decimal Max, string Description);

public static class SettingDefinitions
{
    public static readonly IReadOnlyList<SettingDefinition> All =
    [
        new("lesson.passThresholdPct", 60, 1, 100, "Ngưỡng % để tính là đạt một bài học (BR-CAT, BR-PROG-01)."),
        new("exam.graceSeconds", 30, 0, 300, "Số giây ân hạn khi nộp bài kiểm tra trễ (BR-EXAM)."),
        new("reports.maxPerDay", 10, 1, 100, "Số báo lỗi tối đa mỗi người mỗi ngày (BR-RPT-01)."),
        new("budget.monthlyUsd", 10, 0, 1000, "Trần chi phí dịch vụ trả phí mỗi tháng, USD (BR-CNT-08)."),
        new("infohub.reviewMonths.default", 6, 1, 36, "Chu kỳ rà soát bài Info Hub mặc định, tháng (BR-INFO)."),
        new("infohub.reviewMonths.Ausbildung", 3, 1, 36, "Chu kỳ rà soát bài chủ đề Ausbildung, tháng (BR-INFO)."),
    ];
}
