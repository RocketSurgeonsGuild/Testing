using System.Collections.Immutable;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Code refactoring test result
/// </summary>
/// <param name="ResolvedFixes"></param>
[PublicAPI]
public record CodeRefactoringTestResult(ImmutableArray<ResolvedCodeRefactoringTestResult> ResolvedFixes);