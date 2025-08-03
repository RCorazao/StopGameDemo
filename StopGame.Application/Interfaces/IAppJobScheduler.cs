using System.Linq.Expressions;

namespace StopGame.Application.Interfaces;

public interface IAppJobScheduler
{
    void Enqueue<T>(Expression<Func<T, Task>> methodCall);
}
