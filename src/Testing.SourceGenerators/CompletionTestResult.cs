using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Completion test result
/// </summary>
/// <param name="CompletionLists"></param>
[PublicAPI]
public record CompletionTestResult(ImmutableArray<CompletionListTestResult> CompletionLists);

/// <summary>
///     Completion List Test Result
/// </summary>
/// <param name="Document"></param>
/// <param name="MarkedLocation"></param>
/// <param name="SuggestionModeItem"></param>
/// <param name="Span"></param>
/// <param name="Items"></param>
[PublicAPI]
public record CompletionListTestResult
    (Document Document, MarkedLocation MarkedLocation, CompletionItem? SuggestionModeItem, TextSpan Span, ImmutableArray<CompletionItemTestResult> Items);

/// <summary>
///     Completion Item Test Result
/// </summary>
/// <param name="Item"></param>
/// <param name="Description"></param>
/// <param name="Change"></param>
[PublicAPI]
public record CompletionItemTestResult(CompletionItem Item, CompletionDescription Description, CompletionChange Change);