using FluentAssertions;
using SmartRetry.Impelementations;
using SmartRetry.Models;

namespace SmartRetry.Test.UnitTests;

public class ExponentialBackoffStrategyTests
{
    private readonly ExponentialBackoffStrategy _strategy;
    private readonly RetryOptions _defaultOptions;

    public ExponentialBackoffStrategyTests()
    {
        _strategy = new ExponentialBackoffStrategy();
        _defaultOptions = new RetryOptions
        {
            BaseDelayMs = 100,
            MaxDelayMs = 1000,
            Jitter = JitterStrategy.None
        };
    }

    [Fact]
    public void CalculateDelay_WithNoJitter_ShouldReturnCorrectDelay()
    {
        // Arrange
        var options = _defaultOptions with { Jitter = JitterStrategy.None };

        // Act
        var delay = _strategy.CalculateDelay(100, options, 2);

        // Assert
        delay.Should().Be(400); // Expected delay: 100 * 2^2 = 400
    }

    [Fact]
    public void CalculateDelay_WithFullJitter_ShouldReturnRandomizedDelay()
    {
        // Arrange
        var options = _defaultOptions with { Jitter = JitterStrategy.Full };

        // Act
        var delay = _strategy.CalculateDelay(100, options, 2);

        // Assert
        delay.Should().BeInRange(0, 400); // Random between 0 and 400
    }

    [Fact]
    public void CalculateDelay_WithEqualJitter_ShouldReturnHalfPlusRandom()
    {
        // Arrange
        var options = _defaultOptions with { Jitter = JitterStrategy.Equal };

        // Act
        var delay = _strategy.CalculateDelay(100, options, 2);

        // Assert
        delay.Should().BeInRange(200, 400); // 400/2 + Random(0, 400/2)
    }

    [Fact]
    public void CalculateDelay_WithDecorrelatedJitter_ShouldReturnRandomBetweenBaseAndTriplePrevious()
    {
        // Arrange
        var options = _defaultOptions with { Jitter = JitterStrategy.Decorrelated };

        // Act
        var delay = _strategy.CalculateDelay(200, options, 1);

        // Assert
        delay.Should().BeInRange(100, 600); // Random between BaseDelayMs (100) and previousDelay * 3 (200 * 3)
        delay.Should().BeLessThanOrEqualTo(1000); // Limited by MaxDelayMs
    }

    [Fact]
    public void CalculateDelay_ShouldRespectMaxDelayMs()
    {
        // Arrange
        var options = _defaultOptions with { Jitter = JitterStrategy.None };

        // Act
        var delay = _strategy.CalculateDelay(100, options, 5);

        // Assert
        delay.Should().Be(1000); // Limited by MaxDelayMs (1000)
    }

    [Fact]
    public void CalculateDelay_WithZeroBaseDelay_ShouldReturnZero()
    {
        // Arrange
        var options = _defaultOptions with { BaseDelayMs = 0, Jitter = JitterStrategy.None };

        // Act
        var delay = _strategy.CalculateDelay(0, options, 2);

        // Assert
        delay.Should().Be(0);
    }
}

