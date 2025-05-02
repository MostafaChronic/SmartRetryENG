using Microsoft.Extensions.DependencyInjection;
using SmartRetry;
using SmartRetry.Abstractions;
using SmartRetry.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Set up Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();

        // 2. Resolve Dependencies
        var backoffStrategy = serviceProvider.GetRequiredService<IBackoffStrategy>();

        var retryOptions = serviceProvider.GetRequiredService<RetryOptions>();

        // 3.  Use RetryExecutor
        async Task OperationToRetry()
        {
            Console.WriteLine("Attempting operation...");
            // Simulate a potentially failing operation (e.g., HTTP request)
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync("https://www.google.com/nonexistent"); // Simulate failure
                response.EnsureSuccessStatusCode();
                Console.WriteLine("Operation succeeded!");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Operation failed: {ex.Message}");
                throw; // Re-throw to trigger retry
            }
        }

        try
        {
            await RetryExecutor.ExecuteAsync(OperationToRetry, backoffStrategy, retryOptions);
            Console.WriteLine("Operation completed successfully after retries (if needed).");
        }
        catch (RetryFailedException ex)
        {
            Console.WriteLine($"Operation failed after all retries: {ex.Message}");
            Console.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

    static void ConfigureServices(IServiceCollection services)
    { 
        services.AddSmartRetryWithDefaults(); 
    }
}