using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using SmartRetry.Abstractions;
using SmartRetry.Impelementations;
using SmartRetry.Models;
using Xunit;

namespace SmartRetry.Test.UnitTests
{
    public class SmartRetryConfigurationTests
    {
        [Fact]
        public void AddSmartRetry_WithRetryOptions_SingletonLifetime_RegistersServicesCorrectly()
        { 
            // Arrange
             var services = new ServiceCollection(); var retryOptions = new RetryOptions { MaxRetries = 3, BaseDelayMs = 100, MaxDelayMs = 1000, Jitter = JitterStrategy.Full, ShouldRetryOnException = ex => ex is TimeoutException };

            // Act
            services.AddSmartRetry(ServiceLifetime.Singleton, retryOptions);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var registeredOptions = serviceProvider.GetRequiredService<RetryOptions>();
            var backoffStrategy = serviceProvider.GetRequiredService<IBackoffStrategy>();

            Assert.Same(retryOptions, registeredOptions); // Ensure the exact instance is registered
            Assert.IsType<ExponentialBackoffStrategy>(backoffStrategy);
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IBackoffStrategy) && sd.Lifetime == ServiceLifetime.Singleton));
        }

        [Fact]
        public void AddSmartRetry_WithRetryOptions_TransientLifetime_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var retryOptions = new RetryOptions
            {
                MaxRetries = 4,
                BaseDelayMs = 200,
                MaxDelayMs = 2000,
                Jitter = JitterStrategy.None,
                ShouldRetryOnException = ex => ex is HttpRequestException
            };

            // Act
            services.AddSmartRetry(ServiceLifetime.Transient, retryOptions);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var registeredOptions = serviceProvider.GetRequiredService<RetryOptions>();
            var backoffStrategy = serviceProvider.GetRequiredService<IBackoffStrategy>();

            Assert.Same(retryOptions, registeredOptions);
            Assert.IsType<ExponentialBackoffStrategy>(backoffStrategy);
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IBackoffStrategy) && sd.Lifetime == ServiceLifetime.Transient));
        }

        [Fact]
        public void AddSmartRetry_WithConfigureOptions_AppliesConfigurationCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddSmartRetry(ServiceLifetime.Singleton, new RetryOptions
            {
                MaxRetries = 2,
                BaseDelayMs = 150,
                MaxDelayMs = 1500,
                Jitter = JitterStrategy.Decorrelated,
                ShouldRetryOnException = ex => ex is InvalidOperationException
            });
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var registeredOptions = serviceProvider.GetRequiredService<RetryOptions>();
            var backoffStrategy = serviceProvider.GetRequiredService<IBackoffStrategy>();

            Assert.Equal(2, registeredOptions.MaxRetries);
            Assert.Equal(150, registeredOptions.BaseDelayMs);
            Assert.Equal(1500, registeredOptions.MaxDelayMs);
            Assert.Equal(JitterStrategy.Decorrelated, registeredOptions.Jitter);
            Assert.IsType<ExponentialBackoffStrategy>(backoffStrategy);
            Assert.True(registeredOptions.ShouldRetryOnException(new InvalidOperationException()));
            Assert.False(registeredOptions.ShouldRetryOnException(new TimeoutException()));
        }

        [Fact]
        public void AddSmartRetryWithDefaults_SingletonLifetime_RegistersDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddSmartRetryWithDefaults(ServiceLifetime.Singleton);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var registeredOptions = serviceProvider.GetRequiredService<RetryOptions>();
            var backoffStrategy = serviceProvider.GetRequiredService<IBackoffStrategy>();

            Assert.Equal(5, registeredOptions.MaxRetries);
            Assert.Equal(200, registeredOptions.BaseDelayMs);
            Assert.Equal(5000, registeredOptions.MaxDelayMs);
            Assert.Equal(JitterStrategy.Decorrelated, registeredOptions.Jitter);
            Assert.IsType<ExponentialBackoffStrategy>(backoffStrategy);
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IBackoffStrategy) && sd.Lifetime == ServiceLifetime.Singleton));

            // Verify ShouldRetryOnException logic
            Assert.True(registeredOptions.ShouldRetryOnException(new TimeoutException()));
            Assert.True(registeredOptions.ShouldRetryOnException(new HttpRequestException(null, null, HttpStatusCode.InternalServerError)));
            Assert.False(registeredOptions.ShouldRetryOnException(new HttpRequestException(null, null, HttpStatusCode.BadRequest)));
            Assert.False(registeredOptions.ShouldRetryOnException(new InvalidOperationException()));
        }

        [Fact]
        public void AddSmartRetryWithDefaults_TransientLifetime_RegistersDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddSmartRetryWithDefaults(ServiceLifetime.Transient);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var registeredOptions = serviceProvider.GetRequiredService<RetryOptions>();
            var backoffStrategy = serviceProvider.GetRequiredService<IBackoffStrategy>();

            Assert.Equal(5, registeredOptions.MaxRetries);
            Assert.Equal(200, registeredOptions.BaseDelayMs);
            Assert.Equal(5000, registeredOptions.MaxDelayMs);
            Assert.Equal(JitterStrategy.Decorrelated, registeredOptions.Jitter);
            Assert.IsType<ExponentialBackoffStrategy>(backoffStrategy);
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IBackoffStrategy) && sd.Lifetime == ServiceLifetime.Transient));
        }
  
        [Fact]
        public void AddSmartRetryWithDefaults_DefaultLifetime_RegistersSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddSmartRetryWithDefaults(); // Uses default ServiceLifetime.Singleton
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var registeredOptions = serviceProvider.GetRequiredService<RetryOptions>();
            var backoffStrategy = serviceProvider.GetRequiredService<IBackoffStrategy>();

            Assert.Equal(5, registeredOptions.MaxRetries);
            Assert.Equal(200, registeredOptions.BaseDelayMs);
            Assert.Equal(5000, registeredOptions.MaxDelayMs);
            Assert.Equal(JitterStrategy.Decorrelated, registeredOptions.Jitter);
            Assert.IsType<ExponentialBackoffStrategy>(backoffStrategy);
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IBackoffStrategy) && sd.Lifetime == ServiceLifetime.Singleton));
        }
    }

}