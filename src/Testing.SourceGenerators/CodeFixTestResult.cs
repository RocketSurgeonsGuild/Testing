using System.Collections.Immutable;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Code fix test result
/// </summary>
/// <param name="ResolvedFixes"></param>
[PublicAPI]
public record CodeFixTestResult(ImmutableArray<ResolvedCodeFixTestResult> ResolvedFixes);