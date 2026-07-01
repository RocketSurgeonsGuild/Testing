using System.Collections.Concurrent;
using TUnit.Core.Enums;
using TUnit.Core.Interfaces;

namespace Rocket.Surgery.Extensions.Testing;

public partial class TestRecord<TContext>
{
    /// <summary>
    /// Gets the current phase of test execution (Discovery, Execution, Cleanup, etc.).
    /// </summary>
    public TestPhase Phase => ( (ITestExecution)TestContext ).Phase;

    /// <summary>
    /// Gets the test result after execution completes, or null if the test is still running.
    /// </summary>
    public TestResult? Result => ( (ITestExecution)TestContext ).Result;

    /// <summary>
    /// Gets the cancellation token for this test execution.
    /// Check <see cref="CancellationToken.IsCancellationRequested"/> to honor cancellation requests.
    /// </summary>
    public CancellationToken CancellationToken => ( (ITestExecution)TestContext ).CancellationToken;

    /// <summary>
    /// Gets the timestamp when test execution started, or null if not yet started.
    /// </summary>
    public DateTimeOffset? TestStart => ( (ITestExecution)TestContext ).TestStart;

    /// <summary>
    /// Gets the timestamp when test execution ended, or null if not yet completed.
    /// </summary>
    public DateTimeOffset? TestEnd => ( (ITestExecution)TestContext ).TestEnd;

    /// <summary>
    /// Gets the current retry attempt number (0 for first attempt, 1+ for retries).
    /// </summary>
    public int CurrentRetryAttempt => ( (ITestExecution)TestContext ).CurrentRetryAttempt;

    /// <summary>
    /// Gets the results of prior execution attempts that triggered a retry, in attempt order.
    /// Empty when the test was not retried. The final (surviving) attempt is reflected by
    /// <see cref="Result"/> and is not included here — so for a test that ran 3 times,
    /// <c>RetryAttempts.Count</c> is 2 (the two failed prior attempts) and
    /// <see cref="CurrentRetryAttempt"/> is 2 (the zero-based index of the surviving attempt).
    /// Each entry is the <see cref="TestResult"/> that attempt produced before it was retried.
    /// </summary>
    /// <remarks>
    /// This member has no default implementation because <c>ITestExecution</c> targets
    /// netstandard2.0, which predates default interface members. External types that implement
    /// <c>ITestExecution</c> directly must add this property when upgrading — return an empty
    /// list (e.g. <c>Array.Empty&lt;TestResult&gt;()</c>) if retry history is not tracked.
    /// </remarks>
    public IReadOnlyList<TestResult> RetryAttempts => ( (ITestExecution)TestContext ).RetryAttempts;

    /// <summary>
    /// Gets the reason why this test was skipped, or null if not skipped.
    /// </summary>
    public string? SkipReason => ( (ITestExecution)TestContext ).SkipReason;

    /// <summary>
    /// Gets the retry function that determines whether a failed test should be retried.
    /// </summary>
    public Func<TestContext, Exception, int, Task<bool>>? RetryFunc => ( (ITestExecution)TestContext ).RetryFunc;

    /// <summary>
    /// Overrides the test result with a specific state and custom reason.
    /// </summary>
    /// <param name="state">The desired test state (Passed, Failed, Skipped, Timeout, or Cancelled)</param>
    /// <param name="reason">The reason for overriding the result (cannot be empty)</param>
    /// <exception cref="ArgumentException">Thrown when reason is empty, whitespace, or state is invalid (NotStarted, WaitingForDependencies, Queued, Running)</exception>
    /// <exception cref="InvalidOperationException">Thrown when result has already been overridden</exception>
    /// <remarks>
    /// This method can only be called once per test. Subsequent calls will throw an exception.
    /// Only final states are allowed: Passed, Failed, Skipped, Timeout, or Cancelled. Intermediate states like Running, Queued, NotStarted, or WaitingForDependencies are rejected.
    /// The original exception (if any) is preserved in <see cref="TestResult.OriginalException"/>.
    /// When overriding to Failed, the original exception is retained in <see cref="TestResult.Exception"/>.
    /// When overriding to Passed or Skipped, the Exception property is cleared but preserved in OriginalException.
    /// Best practice: Call this from <see cref="ITestEndEventReceiver.OnTestEnd"/> or After(Test) hooks.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Override failed test to passed
    /// public class RetryOnInfrastructureErrorAttribute : Attribute, ITestEndEventReceiver
    /// {
    ///     public ValueTask OnTestEnd(TestContext context)
    ///     {
    ///         if (context.Result?.Exception is HttpRequestException)
    ///         {
    ///             context.Execution.OverrideResult(TestState.Passed, "Infrastructure error - not a test failure");
    ///         }
    ///         return default;
    ///     }
    ///     public int Order => 0;
    /// }
    ///
    /// // Override failed test to skipped
    /// public class IgnoreOnWeekendAttribute : Attribute, ITestEndEventReceiver
    /// {
    ///     public ValueTask OnTestEnd(TestContext context)
    ///     {
    ///         if (context.Result?.State == TestState.Failed &amp;&amp; DateTime.Now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
    ///         {
    ///             context.Execution.OverrideResult(TestState.Skipped, "Failures ignored on weekends");
    ///         }
    ///         return default;
    ///     }
    ///     public int Order => 0;
    /// }
    /// </code>
    /// </example>
    public void OverrideResult(TestState state, string reason) => ( (ITestExecution)TestContext ).OverrideResult(state, reason);

    /// <summary>
    /// Gets or sets a custom hook executor that overrides the default execution behavior for test-level hooks.
    /// Allows wrapping hook execution in custom logic (e.g., running on a specific thread).
    /// </summary>
    public IHookExecutor? CustomHookExecutor { get => ( (ITestExecution)TestContext ).CustomHookExecutor; set => ( (ITestExecution)TestContext ).CustomHookExecutor = value; }

    /// <summary>
    /// Gets or sets whether the test result should be reported to test runners.
    /// Defaults to true. Set to false to suppress reporting for internal or diagnostic tests.
    /// </summary>
    public bool ReportResult { get => ( (ITestExecution)TestContext ).ReportResult; set => ( (ITestExecution)TestContext ).ReportResult = value; }

    /// <summary>
    /// Gets or sets whether the test should be hidden from test discovery/explorer.
    /// Defaults to false. Set to true to hide the test from discovery notifications
    /// while still allowing it to execute when run directly.
    /// </summary>
    public bool IsNotDiscoverable { get => ( (ITestExecution)TestContext ).IsNotDiscoverable; set => ( (ITestExecution)TestContext ).IsNotDiscoverable = value; }

    /// <summary>
    /// Links an external cancellation token to this test's execution token.
    /// Useful for coordinating cancellation across multiple operations or tests.
    /// </summary>
    /// <param name="cancellationToken">The external cancellation token to link</param>
    public void AddLinkedCancellationToken(CancellationToken cancellationToken) => ( (ITestExecution)TestContext ).AddLinkedCancellationToken(cancellationToken);

    /// <summary>
    /// Gets or sets the execution priority for this test.
    /// Higher priority tests may execute before lower priority tests when resources are limited.
    /// </summary>
    public Priority ExecutionPriority { get => ( (ITestParallelization)TestContext ).ExecutionPriority; set => ( (ITestParallelization)TestContext ).ExecutionPriority = value; }

    /// <summary>
    /// Gets the parallel limiter that controls how many tests can run concurrently.
    /// </summary>
    public IParallelLimit? Limiter => ( (ITestParallelization)TestContext ).Limiter;

    /// <summary>
    /// Adds a parallel constraint to this test context.
    /// Multiple constraints can be combined to create complex parallelization rules.
    /// </summary>
    /// <param name="constraint">The constraint to add</param>
    public void AddConstraint(IParallelConstraint constraint) => ( (ITestParallelization)TestContext ).AddConstraint(constraint);

    /// <summary>
    /// Gets the text writer for standard output.
    /// Use this for writing test progress, debugging information, or general output.
    /// Thread-safe for concurrent writes.
    /// </summary>
    public TextWriter StandardOutput => ( (ITestOutput)TestContext ).StandardOutput;

    /// <summary>
    /// Gets the text writer for error output.
    /// Use this for writing error messages, warnings, or diagnostic information.
    /// Thread-safe for concurrent writes.
    /// </summary>
    public TextWriter ErrorOutput => ( (ITestOutput)TestContext ).ErrorOutput;

    /// <summary>
    /// Gets the collection of artifacts (files, screenshots, logs) attached to this test.
    /// Artifacts are preserved after test execution for review and debugging.
    /// </summary>
    public IReadOnlyCollection<Artifact> Artifacts => ( (ITestOutput)TestContext ).Artifacts;

    /// <summary>
    /// Attaches an artifact (file, screenshot, log, etc.) to this test.
    /// Artifacts are preserved after test execution.
    /// Thread-safe for concurrent calls.
    /// </summary>
    /// <param name="artifact">The artifact to attach</param>
    public void AttachArtifact(Artifact artifact) => ( (ITestOutput)TestContext ).AttachArtifact(artifact);

    /// <summary>
    /// Attaches a file as an artifact to this test.
    /// Artifacts are preserved after test execution.
    /// Thread-safe for concurrent calls.
    /// </summary>
    /// <param name="filePath">The path to the file to attach</param>
    /// <param name="displayName">Optional display name for the artifact. Defaults to the file name.</param>
    /// <param name="description">Optional description of the artifact</param>
    public void AttachArtifact(string filePath, string? displayName = null, string? description = null) => ( (ITestOutput)TestContext ).AttachArtifact(filePath, displayName, description);

    /// <summary>
    /// Gets all standard output written during test execution as a single string.
    /// </summary>
    /// <returns>The accumulated standard output</returns>
    public string GetStandardOutput() => ( (ITestOutput)TestContext ).GetStandardOutput();

    /// <summary>
    /// Gets all error output written during test execution as a single string.
    /// </summary>
    /// <returns>The accumulated error output</returns>
    public string GetErrorOutput() => ( (ITestOutput)TestContext ).GetErrorOutput();

    /// <summary>
    /// Writes a line of text to standard output.
    /// Convenience method for StandardOutput.WriteLine(message).
    /// Thread-safe for concurrent calls.
    /// </summary>
    /// <param name="message">The message to write</param>
    public void WriteLine(string message) => StandardOutput.WriteLine(message);

    /// <summary>
    /// Writes a line of text to error output.
    /// Convenience method for ErrorOutput.WriteLine(message).
    /// Thread-safe for concurrent calls.
    /// </summary>
    /// <param name="message">The error message to write</param>
    public void WriteError(string message) => ErrorOutput.WriteLine(message);

    /// <summary>
    /// Gets the unique identifier for the test definition (template/source) that generated this test.
    /// This ID is shared across all instances of parameterized tests.
    /// </summary>
    public string DefinitionId => ( (ITestMetadata)TestContext ).DefinitionId;

    /// <summary>
    /// Gets the detailed metadata about this test, including class type, method info, and arguments.
    /// </summary>
    public TestDetails TestDetails => ( (ITestMetadata)TestContext ).TestDetails;

    /// <summary>
    /// Gets the base name of the test method.
    /// </summary>
    public string TestName => ( (ITestMetadata)TestContext ).TestName;

    /// <summary>
    /// Gets or sets the display name for the test.
    /// When reading, returns the custom display name if set, otherwise computes from test name and arguments.
    /// Setting this value overrides the default generated name.
    /// </summary>
    public string DisplayName { get => ( (ITestMetadata)TestContext ).DisplayName; set => ( (ITestMetadata)TestContext ).DisplayName = value; }

    /// <summary>
    /// Gets or sets the custom display name formatter type used to format test names.
    /// Must implement IDisplayNameFormatter interface.
    /// </summary>
    public Type? DisplayNameFormatter { get => ( (ITestMetadata)TestContext ).DisplayNameFormatter; set => ( (ITestMetadata)TestContext ).DisplayNameFormatter = value; }

    /// <summary>
    /// Gets the collection of tests that this test depends on.
    /// Tests in this collection will execute before this test runs.
    /// </summary>
    public IReadOnlyList<TestDetails> DependsOn => ( (ITestDependencies)TestContext ).DependsOn;

    /// <summary>
    /// Gets the parent test ID if this test is part of a relationship.
    /// </summary>
    public string? ParentTestId => ( (ITestDependencies)TestContext ).ParentTestId;

    /// <summary>
    /// Gets the relationship type of this test.
    /// </summary>
    public TestRelationship Relationship => ( (ITestDependencies)TestContext ).Relationship;

    /// <summary>
    /// Gets all registered tests that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter tests by.</param>
    /// <returns>A read-only list of matching test contexts.</returns>
    public IReadOnlyList<TestContext> GetTests(Func<TestContext, bool> predicate) => ( (ITestDependencies)TestContext ).GetTests(predicate);

    /// <summary>
    /// Gets all registered tests that match the specified test name.
    /// </summary>
    /// <param name="testName">The name of the test method.</param>
    /// <returns>A read-only list of matching test contexts.</returns>
    public IReadOnlyList<TestContext> GetTests(string testName) => ( (ITestDependencies)TestContext ).GetTests(testName);

    /// <summary>
    /// Gets all registered tests that match the specified test name and class type.
    /// </summary>
    /// <param name="testName">The name of the test method.</param>
    /// <param name="classType">The type of the test class.</param>
    /// <returns>A read-only list of matching test contexts.</returns>
    public IReadOnlyList<TestContext> GetTests(string testName, Type classType) => ( (ITestDependencies)TestContext ).GetTests(testName, classType);

    /// <summary>
    /// Gets the underlying concurrent dictionary for direct access.
    /// </summary>
    public ConcurrentDictionary<string, object?> Items => ( (ITestStateBag)TestContext ).Items;

    /// <summary>
    /// Gets or sets a value in the state bag.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    public object? this[string key] => ( (ITestStateBag)TestContext )[key];

    /// <summary>
    /// Gets the number of items in the state bag.
    /// </summary>
    public int Count => ( (ITestStateBag)TestContext ).Count;

    /// <summary>
    /// Gets a value indicating whether the specified key exists in the state bag.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(string key) => ( (ITestStateBag)TestContext ).ContainsKey(key);

    /// <summary>
    /// Gets the value associated with the specified key, or adds it if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get or add.</param>
    /// <param name="valueFactory">The function used to generate a value for the key if it does not exist.</param>
    /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
    /// <exception cref="InvalidCastException">Thrown if a value already exists for the key but is not of type <typeparamref name="T"/>.</exception>
    public T GetOrAdd<T>(string key, Func<string, T> valueFactory) => ( (ITestStateBag)TestContext ).GetOrAdd(key, valueFactory);

    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found and the value is of the correct type; otherwise, the default value for the type of the value parameter.</param>
    /// <returns><c>true</c> if the key was found and the value is of the correct type; otherwise, <c>false</c>.</returns>
    public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value) => ( (ITestStateBag)TestContext ).TryGetValue(key, out value);

    /// <summary>
    /// Attempts to remove a value with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="value">When this method returns, contains the object removed from the bag, or <c>null</c> if the key does not exist.</param>
    /// <returns><c>true</c> if the object was removed successfully; otherwise, <c>false</c>.</returns>
    public bool TryRemove(string key, [MaybeNullWhen(false)] out object? value) => ( (ITestStateBag)TestContext ).TryRemove(key, out value);

    /// <summary>
    /// Gets the event that is raised when the test context is disposed.
    /// </summary>
    public AsyncEvent<TestContext>? OnDispose => ( (ITestEvents)TestContext ).OnDispose;

    /// <summary>
    /// Gets the event that is raised when the test has been registered with the test runner.
    /// </summary>
    public AsyncEvent<TestContext>? OnTestRegistered => ( (ITestEvents)TestContext ).OnTestRegistered;

    /// <summary>
    /// Gets the event that is raised before the test is initialized.
    /// </summary>
    public AsyncEvent<TestContext>? OnInitialize => ( (ITestEvents)TestContext ).OnInitialize;

    /// <summary>
    /// Gets the event that is raised before the test method is invoked.
    /// </summary>
    public AsyncEvent<TestContext>? OnTestStart => ( (ITestEvents)TestContext ).OnTestStart;

    /// <summary>
    /// Gets the event that is raised after the test method has completed.
    /// </summary>
    public AsyncEvent<TestContext>? OnTestEnd => ( (ITestEvents)TestContext ).OnTestEnd;

    /// <summary>
    /// Gets the event that is raised if the test was skipped.
    /// </summary>
    public AsyncEvent<TestContext>? OnTestSkipped => ( (ITestEvents)TestContext ).OnTestSkipped;

    /// <summary>
    /// Gets the event that is raised before a test is retried.
    /// </summary>
    public AsyncEvent<(TestContext TestContext, int RetryAttempt)>? OnTestRetry => ( (ITestEvents)TestContext ).OnTestRetry;

    /// <summary>
    /// Gets a unique identifier for this test instance.
    /// This value is assigned atomically and is guaranteed to be unique across all test instances in the process.
    /// </summary>
    public int UniqueId => ( (ITestIsolation)TestContext ).UniqueId;

    /// <summary>
    /// Creates an isolated name by combining a base name with the test's unique identifier.
    /// Use for database tables, Redis keys, Kafka topics, etc.
    /// </summary>
    /// <param name="baseName">The base name for the resource.</param>
    /// <returns>A unique name in the format "Test_{UniqueId}_{baseName}".</returns>
    /// <example>
    /// <code>
    /// // In a test with UniqueId = 42:
    /// var tableName = TestContext.Current!.Isolation.GetIsolatedName("todos");  // Returns "Test_42_todos"
    /// var topicName = TestContext.Current!.Isolation.GetIsolatedName("orders"); // Returns "Test_42_orders"
    /// </code>
    /// </example>
    public string GetIsolatedName(string baseName) => ( (ITestIsolation)TestContext ).GetIsolatedName(baseName);

    /// <summary>
    /// Creates an isolated prefix using the test's unique identifier.
    /// Use for key prefixes in Redis, Kafka topic prefixes, etc.
    /// </summary>
    /// <param name="separator">The separator character. Defaults to "_".</param>
    /// <returns>A unique prefix in the format "test{separator}{UniqueId}{separator}".</returns>
    /// <example>
    /// <code>
    /// // In a test with UniqueId = 42:
    /// var prefix = TestContext.Current!.Isolation.GetIsolatedPrefix();       // Returns "test_42_"
    /// var dotPrefix = TestContext.Current!.Isolation.GetIsolatedPrefix("."); // Returns "test.42."
    /// </code>
    /// </example>
    public string GetIsolatedPrefix(string separator = "_") => ( (ITestIsolation)TestContext ).GetIsolatedPrefix(separator);
}
