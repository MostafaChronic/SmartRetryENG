
[![NuGet](https://img.shields.io/nuget/v/SmartRetry.svg)](https://www.nuget.org/packages/SmartRetry/)
[![Build Status](https://dev.azure.com/your-organization/your-project/_apis/build/status/your-build-definition-id)](https://dev.azure.com/your-organization/your-project/_build/latest?definitionId=your-build-definition-id)
[![codecov](https://codecov.io/gh/your-username/SmartRetry/branch/main/graph/badge.svg?token=YOUR-CODECOV-TOKEN)](https://codecov.io/gh/your-username/SmartRetry)

# SmartRetry 📦



## Table of Contents

- [Features](#-features)
- [Installation](#-installation)
- [Getting Started](#-getting-started)
  - [Register in `Program.cs`](#-register-in-programcs)
  - [Inject & Use the Retry Executor](#-inject--use-the-retry-executor)
- [Custom Retry Options](#-custom-retry-options)
- [Backoff & Jitter Strategy](#-backoff--jitter-strategy)
- [Running Tests](#-running-tests)
- [Generate Code Coverage Report](#-generate-code-coverage-report)
- [SonarQube Integration](#-sonarqube-integration)
- [Project Structure](#-project-structure)
- [Contributing](#-contributing)
- [License](#-license)


**SmartRetry** is a lightweight, extensible retry mechanism for .NET, designed to handle transient faults with support for exponential backoff and jitter strategies.  
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
dotnet add package SmartRetry
```

---

## ⚡ Getting Started

### 1. Register in `Program.cs`

```csharp
builder.Services.AddSmartRetry(); // Registers SmartRetry services
```

---

## 💉 Inject & Use the Retry Executor

You can inject `IRetryExecutor` into your service or controller:

```csharp
public class MyService
{
    private readonly IRetryExecutor _executor;

    public MyService(IRetryExecutor executor)
    {
        _executor = executor;
    }

    public async Task CallExternalServiceAsync()
    {
        await _executor.ExecuteAsync(
            async () =>
            {
                await httpClient.GetAsync("https://api.example.com");
            });
    }
}
```

---

## ⚙️ Custom Retry Options

You can customize retry behavior using `RetryOptions`:

```csharp
var options = new RetryOptions
{
    MaxRetries = 5,
    BaseDelayMs = 200,
    ShouldRetryOnException = ex => ex is TimeoutException || ex is HttpRequestException
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

---

## 🔍 SonarQube Integration

Optional support for [SonarQube](https://www.sonarqube.org/):

```bash
dotnet sonarscanner begin /k:"SmartRetry" /d:sonar.login="your_token"
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet sonarscanner end /d:sonar.login="your_token"
```

---

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