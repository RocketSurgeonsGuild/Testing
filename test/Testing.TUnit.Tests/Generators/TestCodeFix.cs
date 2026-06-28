using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests.Generators;

[ExportCodeFixProvider(LanguageNames.CSharp)]
internal class TestCodeFix : CodeFixProvider
{
    private static readonly DiagnosticDescriptor _descriptor = new(
        "TEST0002",
        "title",
        "message",
        "category",
        DiagnosticSeverity.Warning,
        true
    );

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["TEST0001"];

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(
            CodeAction.Create("Test Code Action", async ct => context.Document.WithText(( await context.Document.GetTextAsync(ct) ).Replace(0, 0, "NewText"))),
            context.Diagnostics
        );
        return Task.CompletedTask;
    }
}
