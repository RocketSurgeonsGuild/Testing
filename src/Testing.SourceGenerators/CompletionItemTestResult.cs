using Microsoft.CodeAnalysis.Completion;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Completion Item Test Result
/// </summary>
/// <param name="Item"></param>
/// <param name="Description"></param>
/// <param name="Change"></param>
[PublicAPI]
public record CompletionItemTestResult(CompletionItem Item, CompletionDescription Description, CompletionChange Change);