namespace SmartRetry.Models;

public record RetryOptions
{
    public int MaxRetries { get; init; } = 5;
    public int BaseDelayMs { get; init; } = 200;
    public int MaxDelayMs { get; init; } = 10000;
    public JitterStrategy Jitter { get; init; } = JitterStrategy.Full;
    public Func<Exception, bool>? ShouldRetryOnException { get; init; }
}
