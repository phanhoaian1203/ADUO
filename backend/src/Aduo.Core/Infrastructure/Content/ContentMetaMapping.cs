using System.Linq.Expressions;
using Aduo.Core.Common.Content;
using Aduo.Core.Modules.ContentOps.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aduo.Core.Infrastructure.Content;

public static class ContentMetaMapping
{
    /// <summary>
    /// Map <see cref="ContentMeta"/> thành các cột nằm ngay trong bảng nội dung (owned type, chung bảng), kèm FK THẬT
    /// tới sources và licenses (docs/phase-3/02-database-design.md §1.1). Mọi module nội dung gọi một dòng này
    /// thay vì tự khai báo lại: <c>b.ConfigureContentMeta(e =&gt; e.Meta);</c>.
    /// Dùng owned type (không phải complex type) vì chỉ owned type mới khai báo được khóa ngoại cho các cột này.
    /// </summary>
    public static EntityTypeBuilder<T> ConfigureContentMeta<T>(this EntityTypeBuilder<T> b, Expression<Func<T, ContentMeta?>> meta)
        where T : class
    {
        b.OwnsOne(meta, m =>
        {
            m.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            m.Property(x => x.SourceId).HasColumnName("source_id");
            m.Property(x => x.LicenseId).HasColumnName("license_id");
            m.Property(x => x.OriginalRef).HasColumnName("original_ref").HasMaxLength(300);
            m.Property(x => x.Origin).HasColumnName("origin").HasConversion<string>().HasMaxLength(30);
            m.Property(x => x.AiModel).HasColumnName("ai_model").HasMaxLength(100);
            m.Property(x => x.PromptTemplateId).HasColumnName("prompt_template_id");
            m.Property(x => x.ImportBatchId).HasColumnName("import_batch_id");
            m.Property(x => x.ReviewedBy).HasColumnName("reviewed_by");
            m.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
            m.Property(x => x.PublishedAt).HasColumnName("published_at");
            m.Ignore(x => x.AiAssisted);

            // FK thật: một bản ghi nội dung không thể trỏ tới nguồn hoặc giấy phép không tồn tại (BR-CNT-01).
            m.HasOne<Source>().WithMany().HasForeignKey(x => x.SourceId).OnDelete(DeleteBehavior.Restrict);
            m.HasOne<License>().WithMany().HasForeignKey(x => x.LicenseId).OnDelete(DeleteBehavior.Restrict);
            m.HasIndex(x => x.Status).HasDatabaseName($"ix_{typeof(T).Name.ToLowerInvariant()}_status");
        });

        b.Navigation(meta).IsRequired();
        return b;
    }
}
