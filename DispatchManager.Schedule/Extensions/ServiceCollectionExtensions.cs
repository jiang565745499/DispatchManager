
namespace Microsoft.Extensions.DependencyInjection;
/// <summary>
/// Longbow.Tasks 注入服务拓展类
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注入 Longbow.Tasks 服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddScheduleServices(this IServiceCollection services)
    {
        services.AddTaskServices();
        return services;
    }
}
