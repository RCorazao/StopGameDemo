using Microsoft.Extensions.DependencyInjection;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using StopGame.Application.Interfaces;
using StopGame.Infrastructure.Services;
using System.Collections.Generic;

namespace StopGame.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureLocks(this IServiceCollection services, string redisConnectionString)
        {
            services.AddSingleton<IDistributedLockFactory>(provider =>
            {
                var multiplexers = new List<RedLockMultiplexer>
                {
                    ConnectionMultiplexer.Connect(redisConnectionString)
                };
                return RedLockFactory.Create(multiplexers);
            });
            services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();
            return services;
        }
    }
}
