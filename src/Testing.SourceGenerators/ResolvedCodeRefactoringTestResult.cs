using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Resolved code refactoring test result
/// </summary>
/// <param name="Document"></param>
/// <param name="MarkedLocation"></param>
/// <param name="CodeActions"></param>
[PublicAPI]
public record ResolvedCodeRefactoringTestResult(Document Document, MarkedLocation MarkedLocation, ImmutableArray<CodeActionTestResult> CodeActions);