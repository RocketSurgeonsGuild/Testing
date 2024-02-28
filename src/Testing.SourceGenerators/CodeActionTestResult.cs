using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

public record CodeActionTestResult
(
    ProjectChanges Changes,
    Document TargetDocument,
    CodeAction CodeAction,
    ImmutableDictionary<string, ImmutableArray<TextChange>> TextChanges
);