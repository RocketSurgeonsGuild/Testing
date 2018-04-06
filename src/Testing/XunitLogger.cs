using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    public class XunitLogger : ILogger
    {
        private static readonly string[] NewLineChars = { Environment.NewLine };
        private readonly string _category;
        private readonly XunitLoggerProvider _loggerProvider;
        private readonly ITestOutputHelper _output;

        public XunitLogger(ITestOutputHelper output, string category, XunitLoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
            _category = category;
            _output = output;
        }

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel) || _loggerProvider.FilterNames.Any(name => _category.StartsWith(name, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            var firstLinePrefix = $"| {_category} {logLevel}: ";
            var lines = formatter(state, exception).Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            WriteLine(firstLinePrefix + lines.First());

            var additionalLinePrefix = "|" + new string(' ', firstLinePrefix.Length - 1);
            foreach (var line in lines.Skip(1))
            {
                WriteLine(additionalLinePrefix + line);
            }

            if (exception != null)
            {
                lines = exception.ToString().Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                additionalLinePrefix = "| ";
                foreach (var line in lines.Skip(1))
                {
                    WriteLine(additionalLinePrefix + line);
                }
            }
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _loggerProvider.MinLevel;

        public IDisposable BeginScope<TState>(TState state) => new NullScope();

        private void WriteLine(string message)
        {
            try
            {
                _output.WriteLine(message);
            }
            catch (Exception)
            {
                // We could fail because we're on a background thread and our captured ITestOutputHelper is
                // busted (if the test "completed" before the background thread fired).
                // So, ignore this. There isn't really anything we can do but hope the
                // caller has additional loggers registered
            }
        }

        private class NullScope : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
