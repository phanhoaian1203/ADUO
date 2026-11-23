using Aduo.Core.Common.Content;
using Aduo.Core.Common.Settings;
using Aduo.Core.Infrastructure.Storage;
using Aduo.Core.Modules.ContentOps.Import;
using Aduo.Core.Modules.ContentOps.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aduo.Core.Modules.ContentOps;

/// <summary>Đăng ký dịch vụ của module ContentOps (docs/phase-4/modules/02-contentops.md, đợt 2a).</summary>
public static class ContentOpsModule
{
    public static IServiceCollection AddContentOpsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IObjectStorage, LocalFileObjectStorage>(); // sẽ có bản R2 khi chuẩn bị staging
        services.AddMemoryCache();

        services.AddScoped<MediaService>();
        services.AddScoped<MediaQueries>();
        services.AddScoped<ImportService>();
        services.AddScoped<LifecycleService>();
        services.AddScoped<ReviewQueueService>();
        services.AddScoped<SourceService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<ISettings>(sp => sp.GetRequiredService<SettingsService>());
        services.AddScoped<IContentVersioning, ContentVersioning>();
        return services;
    }

    /// <summary>Module nội dung gọi để đăng ký bộ xử lý import cho loại nội dung của mình.</summary>
    public static IServiceCollection AddImportHandler<THandler>(this IServiceCollection services)
        where THandler : class, IImportHandler
        => services.AddScoped<IImportHandler, THandler>();

    /// <summary>Module nội dung gọi để đăng ký một loại nội dung với hàng đợi duyệt và API vòng đời chung.</summary>
    public static IServiceCollection AddContentResource<TResource>(this IServiceCollection services)
        where TResource : class, IContentResource
        => services.AddScoped<IContentResource, TResource>();
}
