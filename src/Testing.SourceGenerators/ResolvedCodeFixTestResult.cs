using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     The results of a specific analyzers execution
/// </summary>
/// <param name="Document"></param>
/// <param name="Diagnostic"></param>
/// <param name="CodeActions"></param>
[PublicAPI]
public record ResolvedCodeFixTestResult(Document Document, Diagnostic Diagnostic, ImmutableArray<CodeActionTestResult> CodeActions);