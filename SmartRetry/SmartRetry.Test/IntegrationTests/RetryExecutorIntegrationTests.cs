using FluentAssertions;
using Moq;
using SmartRetry;
using SmartRetry.Abstractions;
using SmartRetry.Impelementations;
using SmartRetry.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SmartRetry.Test.IntegrationTests;

public class RetryExecutorIntegrationTests
{
    private readonly Mock<IRetryStrategy> _mockStrategy;
    private readonly ExponentialBackoffStrategy _backoffStrategy;
    private readonly RetryOptions _options;

    public RetryExecutorIntegrationTests()
    {
        _mockStrategy = new Mock<IRetryStrategy>();
        _backoffStrategy = new ExponentialBackoffStrategy();
        _options = new RetryOptions
        {
            MaxRetries = 3,
            BaseDelayMs = 100,
            MaxDelayMs = 1000, 
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenActionFailsAndSucceeds_RetriesSuccessfully()
    {
        // Arrange
        var attempts = 0;
        Func<Task> action = async () =>
        {
            attempts++;
            if (attempts == 1)
                throw new TimeoutException("Temporary failure");
            await Task.CompletedTask;
        };

        // Act
        await RetryExecutor.ExecuteAsync(action, _mockStrategy.Object, _backoffStrategy, _options);

        // Assert
        attempts.Should().Be(2); // 1 failure + 1 success
    }

    [Fact]
    public async Task ExecuteAsync_WhenHttpRequestWithServerError_RetriesSuccessfully()
    {
        // Arrange
        var attempts = 0;
        Func<Task> action = async () =>
        {
            attempts++;
            if (attempts < 3)
                throw new HttpRequestException("Server error", null, HttpStatusCode.InternalServerError);
            await Task.CompletedTask;
        };

        // Act
        await RetryExecutor.ExecuteAsync(action, _mockStrategy.Object, _backoffStrategy, _options);

        // Assert
        attempts.Should().Be(3); // 2 failures + 1 success
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomPolicyRejectsException_ShouldThrowImmediately()
    {
        // Arrange
        var options = _options with
        {
            ShouldRetryOnException = ex => ex is TimeoutException
        };
        var action = new Func<Task>(() => throw new HttpRequestException("Server error", null, HttpStatusCode.InternalServerError));

        // Act
        Func<Task> act = async () => await RetryExecutor.ExecuteAsync(action, _mockStrategy.Object, _backoffStrategy, options);

        // Assert
        await act.Should()
            .ThrowAsync<RetryFailedException>()
            .Where(e =>
                e.InnerException is HttpRequestException &&
                e.Message == "Non-retryable exception encountered.");
    }
 
}