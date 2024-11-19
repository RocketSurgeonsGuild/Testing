namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
/// A TUnit test record (custom context really)
/// </summary>
public interface ITUnitTestRecord
{
    /// <summary>
    /// The events
    /// </summary>
    TestContextEvents Events { get; }

    /// <summary>
    /// The test start
    /// </summary>
    DateTimeOffset? TestStart { get; }

    /// <summary>
    /// The test details
    /// </summary>
    TestDetails TestDetails { get; }

    /// <summary>
    /// The current retry attempt
    /// </summary>
    int CurrentRetryAttempt { get; }

    /// <summary>
    /// The formatters
    /// </summary>
    IReadOnlyList<ArgumentDisplayFormatter> ArgumentDisplayFormatters { get; }

    /// <summary>
    /// The timings
    /// </summary>
    IReadOnlyList<Timing> Timings { get; }

    /// <summary>
    /// The object bag
    /// </summary>
    Dictionary<string, object?> ObjectBag { get; }

    /// <summary>
    /// The result
    /// </summary>
    TestResult? Result { get; }

    /// <summary>
    /// Supress results
    /// </summary>
    void SuppressReportingResult();

    /// <summary>
    /// Add an artifact
    /// </summary>
    /// <param name="artifact"></param>
    void AddArtifact(Artifact artifact);
}