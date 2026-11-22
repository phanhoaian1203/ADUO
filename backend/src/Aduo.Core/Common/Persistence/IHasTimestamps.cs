namespace Aduo.Core.Common.Persistence;

/// <summary>
/// Entity có created_at / updated_at. <see cref="Infrastructure.Persistence.AuditInterceptor"/> tự gán khi SaveChanges,
/// nên handler không cần nhớ tự đặt thời gian.
/// </summary>
public interface IHasTimestamps
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset UpdatedAt { get; set; }
}
