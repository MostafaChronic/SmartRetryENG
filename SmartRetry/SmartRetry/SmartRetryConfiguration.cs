using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SmartRetry.Abstractions;
using SmartRetry.Impelementations;
using SmartRetry.Models;

namespace SmartRetry
{
    public static class SmartRetryConfiguration
    {
        public static IServiceCollection AddSmartRetry(
            this IServiceCollection services,
            ServiceLifetime lifetime,
            RetryOptions retryOptions)
        {
            services.AddSingleton(retryOptions);

            // Register Backoff Strategy
            if (lifetime == ServiceLifetime.Singleton)
            {
                services.AddSingleton<IBackoffStrategy, ExponentialBackoffStrategy>();
            }
            else
            {
                services.AddTransient<IBackoffStrategy, ExponentialBackoffStrategy>();
            }

            return services;
        }

        public static IServiceCollection AddSmartRetry(
           this IServiceCollection services,
           ServiceLifetime lifetime,
           Action<RetryOptions> configureOptions)
        {
            var retryOptions = new RetryOptions();
            configureOptions(retryOptions);

            return services.AddSmartRetry(lifetime, retryOptions);
        }

        public static IServiceCollection AddSmartRetryWithDefaults(
             this IServiceCollection services,
             ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {

            return services.AddSmartRetry(lifetime, new RetryOptions
            {
                MaxRetries = 5,
                BaseDelayMs = 200,
                MaxDelayMs = 5000,
                Jitter = JitterStrategy.Decorrelated,
                ShouldRetryOnException = ex => ex is TimeoutException 
                || (ex is HttpRequestException httpEx && httpEx.StatusCode >= HttpStatusCode.InternalServerError)  
            });
 
        }
    }
}
