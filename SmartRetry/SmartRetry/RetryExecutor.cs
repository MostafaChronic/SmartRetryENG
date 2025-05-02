using SmartRetry.Abstractions;
using SmartRetry.Models;

namespace SmartRetry;
public sealed class RetryExecutor
{
    public static async Task ExecuteAsync(
        Func<Task> action,
        IRetryStrategy retryStrategy,
        IBackoffStrategy backoffStrategy,
        RetryOptions options,
        CancellationToken cancellationToken = default)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        int delay = options.BaseDelayMs;
        int attempt = 0;

        while (true)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception ex) when (IsRetryable(ex, options.ShouldRetryOnException))
            {
                if (attempt >= options.MaxRetries)
                    throw new RetryFailedException("Retry limit exceeded.", ex);

                delay = backoffStrategy.CalculateDelay(delay, options, attempt);
                await Task.Delay(delay, cancellationToken);
                attempt++;
            }
            catch (Exception ex)
            {
                throw new RetryFailedException("Non-retryable exception encountered.", ex);
            }
        }
    }

    private static bool IsRetryable(Exception ex, Func<Exception, bool>? customPolicy)
    {
        if (customPolicy != null)
            return customPolicy(ex);

        if (ex is HttpRequestException httpEx)
        {
            int? status = (int?)httpEx.StatusCode;
            return status == null || status >= 500;
        }

        return ex is TimeoutException;
    }
}
