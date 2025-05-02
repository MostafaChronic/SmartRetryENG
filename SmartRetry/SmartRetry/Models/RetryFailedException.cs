namespace SmartRetry.Models;

public sealed class RetryFailedException : Exception
{
    public RetryFailedException(string message, Exception innerException)
        : base(message, innerException) { }
}
