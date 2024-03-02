using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

[ExportCodeFixProvider(LanguageNames.CSharp)]
internal class TestCodeFix : CodeFixProvider
{
    private static DiagnosticDescriptor _descriptor = new(
        "TEST0001",
        "title",
        "message",
        "category",
        DiagnosticSeverity.Warning,
        true
    );

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("TEST0001");

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(
            CodeAction.Create("Test Code Action", async ct => context.Document.WithText(( await context.Document.GetTextAsync(ct) ).Replace(0, 0, "NewText"))),
            context.Diagnostics
        );
        return Task.CompletedTask;
    }
}