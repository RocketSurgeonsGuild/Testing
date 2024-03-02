using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

public static class GeneratorTestContextExtensions
{
    public static GeneratorTestContext IncludeRelatedType(this GeneratorTestContext context, Type type)
    {
        return context with { _relatedTypes = context._relatedTypes.Add(type), };
    }

    public static Task<AnalyzerTestResult> GenerateAnalyzer<T>(this GeneratorTestContext context)
        where T : DiagnosticAnalyzer, new()
    {
        return GenerateAnalyzer(context, typeof(T));
    }

    public static Task<AnalyzerTestResult> GenerateAnalyzer<T>(this GeneratorTestContextBuilder builder)
        where T : DiagnosticAnalyzer, new()
    {
        return GenerateAnalyzer(builder, typeof(T));
    }

    public static Task<AnalyzerTestResult> GenerateAnalyzer(this GeneratorTestContextBuilder builder, Type type)
    {
        return builder.Build().GenerateAnalyzer(type);
    }

    public static async Task<AnalyzerTestResult> GenerateAnalyzer(this GeneratorTestContext context, Type type)
    {
        var result = await context.IncludeRelatedType(type).GenerateAsync();

        return result.AnalyzerResults.TryGetValue(type, out var analyzerResult)
            ? analyzerResult
            : throw new InvalidOperationException("Analyzer not found");
    }

    public static Task<GeneratorTestResult> GenerateSourceGenerator<T>(this GeneratorTestContext context)
        where T : new()
    {
        return GenerateSourceGenerator(context, typeof(T));
    }

    public static Task<GeneratorTestResult> GenerateSourceGenerator<T>(this GeneratorTestContextBuilder builder)
        where T : new()
    {
        return GenerateSourceGenerator(builder, typeof(T));
    }

    public static Task<GeneratorTestResult> GenerateSourceGenerator(this GeneratorTestContextBuilder builder, Type type)
    {
        return builder.Build().GenerateSourceGenerator(type);
    }

    public static async Task<GeneratorTestResult> GenerateSourceGenerator(this GeneratorTestContext context, Type type)
    {
        var result = await context.IncludeRelatedType(type).GenerateAsync();

        return result.Results.TryGetValue(type, out var analyzerResult)
            ? analyzerResult
            : throw new InvalidOperationException("Generator not found");
    }

    public static Task<CodeFixTestResult> GenerateCodeFix<T>(this GeneratorTestContext context)
        where T : CodeFixProvider, new()
    {
        return GenerateCodeFix(context, typeof(T));
    }

    public static Task<CodeFixTestResult> GenerateCodeFix<T>(this GeneratorTestContextBuilder builder)
        where T : CodeFixProvider, new()
    {
        return GenerateCodeFix(builder, typeof(T));
    }

    public static Task<CodeFixTestResult> GenerateCodeFix(this GeneratorTestContextBuilder builder, Type type)
    {
        return builder.Build().GenerateCodeFix(type);
    }

    public static async Task<CodeFixTestResult> GenerateCodeFix(this GeneratorTestContext context, Type type)
    {
        var result = await context.IncludeRelatedType(type).GenerateAsync();

        return result.CodeFixResults.TryGetValue(type, out var analyzerResult)
            ? analyzerResult
            : throw new InvalidOperationException("Code fix not found");
    }

    public static Task<GeneratorTestResults> AddCodeFix<T>(this GeneratorTestResults context)
        where T : CodeFixProvider, new()
    {
        return AddCodeFix(context, new T());
    }

    public static async Task<GeneratorTestResults> AddCodeFix(this GeneratorTestResults context, CodeFixProvider provider)
    {
        var _logger = context.ProjectInformation.Logger;
        var project = context.ProjectInformation.SourceProject;

        _logger.LogInformation("    {CodeFix}", provider.GetType().FullName);

        var resolvedValues = ImmutableArray.CreateBuilder<ResolvedCodeFixTestResult>();

        foreach (var fixableDiagnostic in context.FinalDiagnostics
                                                 .Join(provider.FixableDiagnosticIds, z => z.Id, z => z, (d, _) => d))
        {
            if (project.GetDocument(fixableDiagnostic.Location.SourceTree) is { } document)
            {
                var cab = ImmutableArray.CreateBuilder<(Document Document, CodeAction CodeAction)>();
                var codeFixContext = new CodeFixContext(document, fixableDiagnostic, (a, _) => cab.Add(( document, a )), CancellationToken.None);
                await provider.RegisterCodeFixesAsync(codeFixContext);

                resolvedValues.Add(new(document, fixableDiagnostic, await CreateCodeActionTestResults(project, cab)));
            }
            else if (fixableDiagnostic.Location.SourceTree is null)
            {
                foreach (var doc in project.Documents)
                {
                    var cab = ImmutableArray.CreateBuilder<(Document Document, CodeAction CodeAction)>();
                    var codeFixContext = new CodeFixContext(doc, fixableDiagnostic, (a, _) => cab.Add(( doc, a )), CancellationToken.None);
                    await provider.RegisterCodeFixesAsync(codeFixContext);
                    resolvedValues.Add(new(doc, fixableDiagnostic, await CreateCodeActionTestResults(project, cab)));
                }
            }
        }

        context = context with
        {
            CodeFixResults = context.CodeFixResults.SetItem(
                provider.GetType(),
                new(
                    resolvedValues
                       .OrderBy(z => z.Document.Name)
                       .ThenBy(static z => z.Diagnostic.Location.GetMappedLineSpan().ToString())
                       .ThenBy(static z => z.Diagnostic.Severity)
                       .ThenBy(static z => z.Diagnostic.Id)
                       .ToImmutableArray()
                )
            ),
        };

        return context;
    }

    public static Task<CodeRefactoringTestResult> GenerateCodeRefactoring<T>(this GeneratorTestContext context)
        where T : CodeRefactoringProvider, new()
    {
        return GenerateCodeRefactoring(context, typeof(T));
    }

    public static Task<CodeRefactoringTestResult> GenerateCodeRefactoring<T>(this GeneratorTestContextBuilder builder)
        where T : CodeRefactoringProvider, new()
    {
        return GenerateCodeRefactoring(builder, typeof(T));
    }

    public static Task<CodeRefactoringTestResult> GenerateCodeRefactoring(this GeneratorTestContextBuilder builder, Type type)
    {
        return builder.Build().GenerateCodeRefactoring(type);
    }

    public static async Task<CodeRefactoringTestResult> GenerateCodeRefactoring(this GeneratorTestContext context, Type type)
    {
        var result = await context.IncludeRelatedType(type).GenerateAsync();

        return result.CodeRefactoringResults.TryGetValue(type, out var analyzerResult)
            ? analyzerResult
            : throw new InvalidOperationException("Generator not found");
    }

    public static Task<GeneratorTestResults> AddCodeRefactoring<T>(this GeneratorTestResults context)
        where T : CodeRefactoringProvider, new()
    {
        return AddCodeRefactoring(context, new T());
    }

    public static async Task<GeneratorTestResults> AddCodeRefactoring(this GeneratorTestResults context, CodeRefactoringProvider provider)
    {
        var _logger = context.ProjectInformation.Logger;
        var project = context.ProjectInformation.SourceProject;

        var resolvedValues = ImmutableArray.CreateBuilder<ResolvedCodeRefactoringTestResult>();
        _logger.LogInformation("    {CodeRefactoring}", provider.GetType().FullName);
        foreach (( var path, var mark ) in context.MarkedLocations)
        {
            var cab = ImmutableArray.CreateBuilder<(Document Document, CodeAction CodeAction)>();
            var document = project.Documents.Single(z => z.Name == path);
            var codeFixContext = new CodeRefactoringContext(document, mark.Location, a => cab.Add(( document, a )), CancellationToken.None);
            await provider.ComputeRefactoringsAsync(codeFixContext);
            resolvedValues.Add(new(document, mark, await CreateCodeActionTestResults(project, cab)));
        }

        context = context with
        {
            CodeRefactoringResults = context.CodeRefactoringResults.SetItem(
                provider.GetType(),
                new(
                    resolvedValues
                       .OrderBy(z => z.Document.Name)
                       .ThenBy(z => z.MarkedLocation)
                       .ToImmutableArray()
                )
            ),
        };

        return context;
    }

    private static async Task<ImmutableArray<CodeActionTestResult>> CreateCodeActionTestResults(
        Project project,
        ImmutableArray<(Document Document, CodeAction CodeAction)>.Builder cab
    )
    {
        var originalSolution = project.Solution.Workspace.CurrentSolution;
        var results = ImmutableArray.CreateBuilder<CodeActionTestResult>();
        foreach (( var targetDocument, var codeAction ) in cab)
        {
            var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
            foreach (var o in operations)
            {
                o.Apply(originalSolution.Workspace, CancellationToken.None);
            }

            var changedProject = project.Solution.Workspace.CurrentSolution.GetProject(project.Id);
            if (changedProject is null) continue;

            var documentChanges = changedProject.GetChanges(project);

            var documentTextChanges = ImmutableDictionary.CreateBuilder<string, ImmutableArray<TextChange>>();

            foreach (var changedDocument in documentChanges.GetChangedDocuments(true))
            {
                var oldDocument = project.GetDocument(changedDocument);
                var newDocument = changedProject.GetDocument(changedDocument);
                var textChanges = ( await newDocument!.GetTextChangesAsync(oldDocument!) ).ToImmutableArray();

                documentTextChanges.Add(oldDocument.Name, textChanges);
            }

            originalSolution.Workspace.TryApplyChanges(originalSolution);
            results.Add(new(documentChanges, targetDocument, codeAction, documentTextChanges.ToImmutable()));
        }

        return results
              .OrderBy(z => z.TargetDocument.Name)
              .ThenBy(z => z.CodeAction.Title)
              .ToImmutableArray();
    }
}