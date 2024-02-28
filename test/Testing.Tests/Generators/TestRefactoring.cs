using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

internal class TestRefactoring : CodeRefactoringProvider
{
    public override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        context.RegisterRefactoring(
            CodeAction.Create("Test Code Action", async ct => context.Document.WithText(( await context.Document.GetTextAsync(ct) ).Replace(0, 0, "NewText")))
        );
        return Task.CompletedTask;
    }
}

internal class TestCompletion : CompletionProvider
{
    public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options)
    {
        return true;
    }

    public override Task<CompletionDescription> GetDescriptionAsync(Document document, CompletionItem item, CancellationToken cancellationToken)
    {
        return base.GetDescriptionAsync(document, item, cancellationToken);
    }

    public override Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item, char? commitKey, CancellationToken cancellationToken)
    {
        return base.GetChangeAsync(document, item, commitKey, cancellationToken);
    }

    public override Task ProvideCompletionsAsync(CompletionContext context)
    {
        context.AddItem(CompletionItem.Create("Test Completion"));
        return Task.CompletedTask;
    }
}