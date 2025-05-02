using System.Net;
using FluentAssertions;
using Moq;
using SmartRetry.Abstractions;
using SmartRetry.Models;

namespace SmartRetry.Test.UnitTests;

public class NonRetryableExceptionsTest
{
     
    private readonly Mock<IBackoffStrategy> _mockBackoff;
    private readonly RetryOptions _options;

    public NonRetryableExceptionsTest()
    { 
        _mockBackoff = new Mock<IBackoffStrategy>();
        _options = new RetryOptions
        {
            MaxRetries = 3,
            BaseDelayMs = 100,
            MaxDelayMs = 1000
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenNonRetryableExceptionOccurs_ShouldThrowRetryFailedException()
    {
        // Arrange
        var action = new Func<Task>(() => throw new ArgumentException("Non-retryable exception"));

        // Act
        Func<Task> act = async () => await RetryExecutor.ExecuteAsync(action, _mockBackoff.Object, _options);

        // Assert 
        await act.Should()
        .ThrowAsync<RetryFailedException>()
        .Where(e =>
            e.InnerException is ArgumentException &&
            e.Message == "Non-retryable exception encountered.");
 
    }

    [Fact]
    public async Task ExecuteAsync_WhenHttpClientErrorOccurs_ShouldThrowRetryFailedException()
    {
        // Arrange
        var action = new Func<Task>(() => throw new HttpRequestException("Client error", null, HttpStatusCode.BadRequest));

        // Act
        Func<Task> act = async () => await RetryExecutor.ExecuteAsync(action, _mockBackoff.Object, _options);

        // Assert 
        await act.Should()
            .ThrowAsync<RetryFailedException>()
            .Where(e =>
            e.InnerException is HttpRequestException &&
            e.Message == "Non-retryable exception encountered.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenNullActionProvided_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<Task> action = null;

        // Act
        Func<Task> act = async () => await RetryExecutor.ExecuteAsync(action, _mockBackoff.Object, _options);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
