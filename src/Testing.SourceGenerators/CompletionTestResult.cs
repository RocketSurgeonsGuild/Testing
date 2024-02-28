using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

[PublicAPI]
public record CompletionTestResult(ImmutableArray<CompletionListTestResult> CompletionLists);

[PublicAPI]
public record CompletionListTestResult
    (Document Document, MarkedLocation MarkedLocation, CompletionItem? SuggestionModeItem, TextSpan Span, ImmutableArray<CompletionItemTestResult> Items);

[PublicAPI]
public record CompletionItemTestResult(CompletionItem Item, CompletionDescription Description, CompletionChange Change);