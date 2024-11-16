using JetBrains.Annotations;
using Serilog;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     Defaults to be configured globally for tests
/// </summary>
[PublicAPI]
public static class RocketSurgeonsTestingDefaults
{
    /// <summary>
    ///     Default log format
    /// </summary>
    public static string LogFormat { get; set; } = "[{Level:w4}] {Message}{NewLine}{Exception}";

    /// <summary>
    ///     The default delegate for configuring the logger.
    /// </summary>
    public static Action<LoggerConfiguration>? ConfigureLogging { get; set; }
}