using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

class TestCodeFix : CodeFixProvider
{
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(CodeAction.Create("Test Code Action", async ct => context.Document.WithText((await context.Document.GetTextAsync(ct)).Replace(0, 0, "NewText"))), context.Diagnostics);
        return Task.CompletedTask;
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("TEST0001");

    private static DiagnosticDescriptor _descriptor = new DiagnosticDescriptor(
        "TEST0001",
        "title",
        "message",
        "category",
        DiagnosticSeverity.Warning,
        true
    );
}

class TestRefactoring : CodeRefactoringProvider
{
    public override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        context.RegisterRefactoring(CodeAction.Create("Test Code Action", async ct => context.Document.WithText((await context.Document.GetTextAsync(ct)).Replace(0, 0, "NewText"))));
        return Task.CompletedTask;
    }
}
