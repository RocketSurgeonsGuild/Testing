using System.Collections.Immutable;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Completion test result
/// </summary>
/// <param name="CompletionLists"></param>
[PublicAPI]
public record CompletionTestResult(ImmutableArray<CompletionListTestResult> CompletionLists);