using System.Net;
using FluentAssertions;
using Moq;
using SmartRetry.Abstractions;
using SmartRetry.Impelementations;
using SmartRetry.Models;

namespace SmartRetry.Test.UnitTests;

public class RetryExecutorTests
{
     
    private readonly Mock<IBackoffStrategy> _mockBackoff;
    private readonly RetryOptions _options;
    private readonly ExponentialBackoffStrategy _backoffStrategy;

    public RetryExecutorTests()
    { 
        _mockBackoff = new Mock<IBackoffStrategy>();
        _options = new RetryOptions
        {
            MaxRetries = 3,
            BaseDelayMs = 100,
            MaxDelayMs = 1000
        };
        _backoffStrategy = new ExponentialBackoffStrategy();
    }


    [Fact]
    public async Task ExecuteAsync_WhenActionSucceeds_NoRetryOccurs()
    {
        // Arrange
        var wasCalled = false;
        Func<Task> action = async () =>
        {
            wasCalled = true;
            await Task.CompletedTask;
        };

        // Act
        await RetryExecutor.ExecuteAsync(action, _mockBackoff.Object, _options);

        // Assert
        wasCalled.Should().BeTrue(); 
    }

    [Fact]
    public async Task ExecuteAsync_WhenRetryableExceptionOccurs_ShouldRetryAndSucceed()
    {
        // Arrange
        var attempts = 0;
        Func<Task> action = async () =>
        {
            attempts++;
            if (attempts < 2)
                throw new TimeoutException("Temporary failure");
            await Task.CompletedTask;
        };

        // Act
        await RetryExecutor.ExecuteAsync(action, _backoffStrategy, _options);

        // Assert
        attempts.Should().Be(2); // 1 failure + 1 success
    }

    [Fact]
    public async Task ExecuteAsync_WhenMaxRetriesExceeded_ShouldThrowRetryFailedException()
    {
        // Arrange
        var action = new Func<Task>(() => throw new HttpRequestException("Server error", null, HttpStatusCode.InternalServerError));

        // Act
        Func<Task> act = async () => await RetryExecutor.ExecuteAsync(action, _backoffStrategy, _options);

        // Assert
        await act.Should()
           .ThrowAsync<RetryFailedException>()
            .Where(e =>
            e.InnerException is HttpRequestException &&
            e.Message == "Retry limit exceeded.");
 
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var action = new Func<Task>(() =>
        {
            cts.Cancel();
            throw new HttpRequestException("Server error", null, HttpStatusCode.InternalServerError);
        });

        // Act
        Func<Task> act = async () => await RetryExecutor.ExecuteAsync(action, _backoffStrategy, _options, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomRetryPolicyProvided_ShouldRespectPolicy()
    {
        // Arrange
        var options = _options with
        {
            ShouldRetryOnException = ex => ex is InvalidOperationException
        };
        var attempts = 0;
        Func<Task> action = async () =>
        {
            attempts++;
            if (attempts < 2)
                throw new InvalidOperationException("Custom error");
            await Task.CompletedTask;
        };

        // Act
        await RetryExecutor.ExecuteAsync(action, _backoffStrategy, options);

        // Assert
        attempts.Should().Be(2); // 1 failure + 1 success
    }

}


