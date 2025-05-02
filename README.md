
# SmartRetry: Easy Retry Logic for .NET

## Table of Contents

- [Features](#-features)
- [Installation](#-installation)
- [Getting Started](#-getting-started)
  - [Register in `Program.cs`](#1-register-in-programcs)
  - [Inject & Use the Retry Executor](#2-inject--use-the-retry-executor)
- [Custom Retry Options](#-custom-retry-options)
- [Backoff & Jitter Strategy](#-backoff--jitter-strategy)


**SmartRetry** is a lightweight and extensible .NET library designed to simplify the implementation of retry logic with built-in support for exponential backoff and jitter strategies.
Ideal for HTTP calls, database retries, and other retryable operations.

---

## 🚀 Features

- ✅ Retry execution with custom logic
- 🔁 Exponential backoff & jitter support
- ⚙️ Configurable via `RetryOptions`
- 💉 Clean Dependency Injection (DI) integration
- 🧪 Unit & integration tested
- 🛡️ Follows SOLID design principles

---

## 📦 Installation

Install via [NuGet](https://www.nuget.org/):

```bash
dotnet add package Engine.SmartRetry --version 1.0.1
```

---

## ⚡ Getting Started  

### 1. Register in `Program.cs`

```csharp
builder.Services.AddSmartRetry(); // Registers SmartRetry services
```

---

## 💉 Inject & Use the Retry Executor

You can use `IBackoffStrategy` into your app:
For Exmaple Console application

```csharp

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
```

---

## ⚙️ Custom Retry Options

You can customize retry behavior using `RetryOptions`:

```csharp
var options = new RetryOptions
{
    MaxRetries = 1,
    BaseDelayMs = 100,
    MaxDelayMs = 2000,
    Jitter = JitterStrategy.Decorrelated,
    ShouldRetryOnException = ex => ex is TimeoutException 
    || (ex is HttpRequestException httpEx && httpEx.StatusCode >= HttpStatusCode.InternalServerError) 
};

await _executor.ExecuteAsync(
    async () => await httpClient.GetAsync("https://api.example.com"),
    options: options
);

```


---

## 🔁 Backoff & Jitter Strategy

Built-in backoff strategies include:

- Exponential backoff
- Full jitter / equal jitter

You can implement your own by extending `IBackoffStrategy`.

---

## 🧪 Running Tests

### Run Unit & Integration Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

The test results will be stored in a subfolder of `/TestResults/`.

---

## 📊 Generate Code Coverage Report

Use tools like [ReportGenerator](https://github.com/danielpalme/ReportGenerator):

```bash
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
```

Then open `coverage-report/index.html` in your browser to view results.

 

## 📁 Project Structure

```
SmartRetry/
│
├── Abstractions/         # Interfaces like IRetryStrategy, IBackoffStrategy
├── Models/               # RetryOptions, RetryFailedException
├── Strategies/           # Built-in retry/backoff strategies
├── SmartRetry.csproj     # Main library
│
├── SmartRetry.Test/              # Unit tests
├── SmartRetry.IntegrationTests/  # Integration tests
```

---

## 🤝 Contributing

Feel free to submit issues or pull requests. All contributions are welcome and appreciated!

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).