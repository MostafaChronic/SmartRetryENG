using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartRetry.Models;

namespace SmartRetry.Abstractions;

public interface IBackoffStrategy
{
    int CalculateDelay(int previousDelay, RetryOptions options, int attempt);
}
