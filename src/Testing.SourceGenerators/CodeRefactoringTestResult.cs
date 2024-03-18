using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Code refactoring test result
/// </summary>
/// <param name="ResolvedFixes"></param>
[PublicAPI]
public record CodeRefactoringTestResult(ImmutableArray<ResolvedCodeRefactoringTestResult> ResolvedFixes);

/// <summary>
///     Resolved code refactoring test result
/// </summary>
/// <param name="Document"></param>
/// <param name="MarkedLocation"></param>
/// <param name="CodeActions"></param>
[PublicAPI]
public record ResolvedCodeRefactoringTestResult(Document Document, MarkedLocation MarkedLocation, ImmutableArray<CodeActionTestResult> CodeActions);