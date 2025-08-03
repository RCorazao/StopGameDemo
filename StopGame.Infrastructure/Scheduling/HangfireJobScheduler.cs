using Hangfire;
using StopGame.Application.Interfaces;
using System.Linq.Expressions;

namespace StopGame.Infrastructure.Scheduling;

public class HangfireJobScheduler : IAppJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireJobScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        _backgroundJobClient.Enqueue(methodCall);
    }
}
