using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     A code action test result
/// </summary>
/// <param name="Changes"></param>
/// <param name="TargetDocument"></param>
/// <param name="CodeAction"></param>
/// <param name="TextChanges"></param>
public record CodeActionTestResult
(
    ProjectChanges Changes,
    Document TargetDocument,
    CodeAction CodeAction,
    ImmutableDictionary<string, ImmutableArray<TextChange>> TextChanges
);