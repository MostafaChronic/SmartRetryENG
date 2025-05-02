using SmartRetry.Abstractions;
using SmartRetry.Models;

namespace SmartRetry.Impelementations;

public class ExponentialBackoffStrategy : IBackoffStrategy
{
    private static readonly Random _random = new();

    public int CalculateDelay(int previousDelay, RetryOptions options, int attempt)
    {
        int exponentialDelay = Math.Min(options.MaxDelayMs, options.BaseDelayMs * (int)Math.Pow(2, attempt));

        return options.Jitter switch
        {
            JitterStrategy.None => exponentialDelay,
            JitterStrategy.Full => _random.Next(0, exponentialDelay),
            JitterStrategy.Equal => exponentialDelay / 2 + _random.Next(0, exponentialDelay / 2),
            JitterStrategy.Decorrelated => Math.Min(options.MaxDelayMs, _random.Next(options.BaseDelayMs, previousDelay * 3)),
            _ => exponentialDelay
        };
    }
}
