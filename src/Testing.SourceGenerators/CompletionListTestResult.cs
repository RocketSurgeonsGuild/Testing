using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

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