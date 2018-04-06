using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;

        public XunitLoggerProvider(ITestOutputHelper output)
            : this(output, LogLevel.Information)
        {
        }

        public XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel)
        {
            _output = output;
            MinLevel = minLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(_output, categoryName, this);
        }

        public LogLevel MinLevel { get; set; }
        public HashSet<string> FilterNames { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public void Dispose()
        {
        }
    }
}
