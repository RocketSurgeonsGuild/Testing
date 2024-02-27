using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

[PublicAPI]
public record CodeFixTestResult(ImmutableArray<ResolvedCodeFixTestResult> ResolvedFixes);

[PublicAPI]
public record ResolvedCodeFixTestResult(Document Document, Diagnostic Diagnostic, ImmutableArray<CodeActionTestResult> CodeActions);

