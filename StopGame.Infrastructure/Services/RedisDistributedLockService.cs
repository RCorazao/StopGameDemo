using RedLockNet;
using StopGame.Application.Interfaces;

namespace StopGame.Infrastructure.Services
{
    public class RedisDistributedLockService : IDistributedLockService
    {
        private readonly IDistributedLockFactory _lockFactory;

        public RedisDistributedLockService(IDistributedLockFactory lockFactory)
        {
            _lockFactory = lockFactory;
        }

        public async Task<IDisposable?> AcquireLockAsync(string key, TimeSpan expiry, TimeSpan wait, TimeSpan retry, CancellationToken cancellationToken = default)
        {
            var redLock = await _lockFactory.CreateLockAsync(key, expiry, wait, retry, cancellationToken);
            return redLock.IsAcquired ? redLock : null;
        }
    }
}
