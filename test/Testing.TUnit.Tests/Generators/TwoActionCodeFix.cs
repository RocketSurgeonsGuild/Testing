using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests.Generators;

/// <summary>
///     Offers two code actions for the same diagnostic, each inserting different text, so each action's diff can be told apart.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp)]
internal class TwoActionCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["TEST0001"];

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.RegisterCodeFix(Insert(context.Document, "Insert A", "/* A */"), context.Diagnostics);
        context.RegisterCodeFix(Insert(context.Document, "Insert B", "/* B */"), context.Diagnostics);
        return Task.CompletedTask;
    }

    private static CodeAction Insert(Document document, string title, string text) =>
        CodeAction.Create(title, async ct => document.WithText(( await document.GetTextAsync(ct) ).Replace(0, 0, text)), title);
}
