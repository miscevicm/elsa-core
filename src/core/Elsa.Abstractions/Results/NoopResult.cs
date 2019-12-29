﻿using Elsa.Services;
using Elsa.Services.Models;

namespace Elsa.Results
{
    /// <summary>
    /// A result that does nothing.
    /// </summary>
    public class NoopResult : ActivityExecutionResult
    {
        protected override void Execute(IProcessRunner runner, ProcessExecutionContext processContext)
        {
            // Noop.
        }
    }
}
