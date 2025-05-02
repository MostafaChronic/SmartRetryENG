using SmartRetry.Models;

namespace SmartRetry.Abstractions;

public interface IRetryStrategy
{
    Task ExecuteAsync(Func<Task> action, RetryOptions options, CancellationToken cancellationToken = default);
}
