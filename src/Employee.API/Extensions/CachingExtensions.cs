using Microsoft.Extensions.Caching.Distributed;

namespace Employee.API.Extensions
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddCachingServices(
            this IServiceCollection services,
            IConfiguration configuration = null)
        {
            var cacheSettings = configuration?.GetSection("Cache");
            var cacheType = cacheSettings?.GetValue<string>("Type") ?? "Memory";

            if (cacheType.Equals("Redis", StringComparison.OrdinalIgnoreCase))
            {
                var redisConnectionString = configuration?.GetConnectionString("Redis") 
                    ?? throw new InvalidOperationException("Redis connection string not configured");
                
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });
            }
            else
            {
                services.AddMemoryCache();
                services.AddDistributedMemoryCache();
            }

            return services;
        }
    }
}
