using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests.Generators;

[ExportCodeRefactoringProvider(LanguageNames.CSharp)]
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