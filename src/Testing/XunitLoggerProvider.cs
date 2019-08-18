using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// The default logging provider for XUnit
    /// </summary>
    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// The default constructor with min level
        /// </summary>
        /// <param name="output"></param>
        /// <param name="minLevel"></param>
        public XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel)
        {
            _output = output;
            MinLevel = minLevel;
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(_output, categoryName, this);
        }

        /// <summary>
        /// The current level of the logger
        /// </summary>
        /// <value></value>
        public LogLevel MinLevel { get; set; }

        /// <summary>
        /// The current filter of the logger
        /// </summary>
        /// <returns></returns>
        public HashSet<string> FilterNames { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
