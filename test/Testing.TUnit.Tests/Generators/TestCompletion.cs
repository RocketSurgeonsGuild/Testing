using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests.Generators;

[ExportCompletionProvider(nameof(TestCompletion), LanguageNames.CSharp)]
internal class TestCompletion : CompletionProvider
{
    public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options) => true;

    public override Task ProvideCompletionsAsync(CompletionContext context)
    {
        context.AddItem(CompletionItem.Create("Test Completion"));
        return Task.CompletedTask;
    }
}
