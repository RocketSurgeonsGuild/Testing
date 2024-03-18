using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Code fix test result
/// </summary>
/// <param name="ResolvedFixes"></param>
[PublicAPI]
public record CodeFixTestResult(ImmutableArray<ResolvedCodeFixTestResult> ResolvedFixes);

/// <summary>
///     The results of a specific analyzers execution
/// </summary>
/// <param name="Document"></param>
/// <param name="Diagnostic"></param>
/// <param name="CodeActions"></param>
[PublicAPI]
public record ResolvedCodeFixTestResult(Document Document, Diagnostic Diagnostic, ImmutableArray<CodeActionTestResult> CodeActions);