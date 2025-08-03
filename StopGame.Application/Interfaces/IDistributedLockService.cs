
namespace StopGame.Application.Interfaces
{
    public interface IDistributedLockService
    {
        /// <summary>
        /// Attempts to acquire a distributed lock for the given key.
        /// </summary>
        /// <param name="key">Lock key</param>
        /// <param name="expiry">How long the lock is held for</param>
        /// <param name="wait">How long to wait to acquire the lock</param>
        /// <param name="retry">How often to retry</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A disposable lock handle, or null if not acquired</returns>
        Task<IDisposable?> AcquireLockAsync(string key, TimeSpan expiry, TimeSpan wait, TimeSpan retry, CancellationToken cancellationToken = default);
    }
}
