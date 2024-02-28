using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

[PublicAPI]
public record CodeRefactoringTestResult(ImmutableArray<ResolvedCodeRefactoringTestResult> ResolvedFixes);

[PublicAPI]
public record ResolvedCodeRefactoringTestResult(Document Document, MarkedLocation MarkedLocation, ImmutableArray<CodeActionTestResult> CodeActions);