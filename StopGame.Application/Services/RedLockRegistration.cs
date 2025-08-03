using Microsoft.Extensions.DependencyInjection;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace StopGame.Application.Services
{
    public static class RedLockRegistration
    {
        public static IServiceCollection AddRedLock(this IServiceCollection services, string redisConnectionString)
        {
            services.AddSingleton<IDistributedLockFactory>(provider =>
            {
                var multiplexers = new List<RedLockMultiplexer>
                {
                    ConnectionMultiplexer.Connect(redisConnectionString)
                };
                return RedLockFactory.Create(multiplexers);
            });
            return services;
        }
    }
}
