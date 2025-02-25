using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
// ReSharper disable UseCollectionExpression

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Generator test context extensions
/// </summary>
public static class GeneratorTestContextExtensions
{
    /// <summary>
    ///     Include a related type
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static GeneratorTestContext IncludeRelatedType(this GeneratorTestContext context, Type type)
    {
        return context with { _relatedTypes = context._relatedTypes.Add(type), };
    }

    /// <summary>
    ///     Generate the analyzer
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<AnalyzerTestResult> GenerateAnalyzer<T>(this GeneratorTestContext context, CancellationToken cancellationToken = default)
        where T : DiagnosticAnalyzer, new()
    {
        return GenerateAnalyzer(context, typeof(T), cancellationToken);
    }

    /// <summary>
    ///     Generate the analyzer
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<AnalyzerTestResult> GenerateAnalyzer<T>(this GeneratorTestContextBuilder builder, CancellationToken cancellationToken = default)
        where T : DiagnosticAnalyzer, new()
    {
        return GenerateAnalyzer(builder, typeof(T), cancellationToken);
    }

    /// <summary>
    ///     Generate the analyzer
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<AnalyzerTestResult> GenerateAnalyzer(this GeneratorTestContextBuilder builder, Type type, CancellationToken cancellationToken = default)
    {
        return builder.Build().GenerateAnalyzer(type, cancellationToken);
    }

    /// <summary>
    ///     Generate the analyzer
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<AnalyzerTestResult> GenerateAnalyzer(this GeneratorTestContext context, Type type, CancellationToken cancellationToken = default)
    {
        var result = await context.IncludeRelatedType(type).GenerateAsync(cancellationToken);

        return result.AnalyzerResults.TryGetValue(type, out var analyzerResult)
            ? analyzerResult
            : throw new InvalidOperationException("Analyzer not found");
    }

    /// <summary>
    ///     Generate the source generator
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<GeneratorTestResult> GenerateSourceGenerator<T>(this GeneratorTestContext context, CancellationToken cancellationToken = default)
        where T : new()
    {
        return GenerateSourceGenerator(context, typeof(T), cancellationToken);
    }

    /// <summary>
    ///     Generate the source generator
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<GeneratorTestResult> GenerateSourceGenerator<T>(this GeneratorTestContextBuilder builder, CancellationToken cancellationToken = default)
        where T : new()
    {
        return GenerateSourceGenerator(builder, typeof(T), cancellationToken);
    }

    /// <summary>
    ///     Generate the source generator
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<GeneratorTestResult> GenerateSourceGenerator(this GeneratorTestContextBuilder builder, Type type, CancellationToken cancellationToken = default)
    {
        return builder.Build().GenerateSourceGenerator(type, cancellationToken);
    }

    /// <summary>
    ///     Generate the source generator
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<GeneratorTestResult> GenerateSourceGenerator(this GeneratorTestContext context, Type type, CancellationToken cancellationToken = default)
    {
        var result = await context.IncludeRelatedType(type).GenerateAsync(cancellationToken);

        return result.Results.TryGetValue(type, out var analyzerResult)
            ? analyzerResult
            : throw new InvalidOperationException("Generator not found");
    }

    /// <summary>
    ///     Generate the code fix
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<CodeFixTestResult> GenerateCodeFix<T>(this GeneratorTestContext context, CancellationToken cancellationToken = default)
        where T : CodeFixProvider, new()
    {
        return GenerateCodeFix(context, typeof(T), cancellationToken);
    }

    /// <summary>
    ///     Generate the code fix
    /// </summary>
    /// <param name="builder"></param>
    /// <typeparam name="T"></typeparam>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<CodeFixTestResult> GenerateCodeFix<T>(this GeneratorTestContextBuilder builder, CancellationToken cancellationToken = default)
        where T : CodeFixProvider, new()
    {
        return GenerateCodeFix(builder, typeof(T), cancellationToken);
    }

    /// <summary>
    ///     Generate the code fix
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<CodeFixTestResult> GenerateCodeFix(this GeneratorTestContextBuilder builder, Type type, CancellationToken cancellationToken = default)
    {
        return builder.Build().GenerateCodeFix(type, cancellationToken);
    }

    /// <summary>
    ///     Generate the code fix
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<CodeFixTestResult> GenerateCodeFix(this GeneratorTestContext context, Type type, CancellationToken cancellationToken = default)
    {
        var result = await context.IncludeRelatedType(type).GenerateAsync(cancellationToken);

        return result.CodeFixResults.TryGetValue(type, out var analyzerResult)
            ? analyzerResult
            : throw new InvalidOperationException("Code fix not found");
    }

    /// <summary>
    ///     Add the code fix to the results
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<GeneratorTestResults> AddCodeFix<T>(this GeneratorTestResults context, CancellationToken cancellationToken = default)
        where T : CodeFixProvider, new()
    {
        return AddCodeFix(context, new T(), cancellationToken);
    }

    /// <summary>
    ///     Add the code fix to the results
    /// </summary>
    /// <param name="context"></param>
    /// <param name="provider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<GeneratorTestResults> AddCodeFix(this GeneratorTestResults context, CodeFixProvider provider, CancellationToken cancellationToken = default)
    {
        var _logger = context.ProjectInformation.Logger;
        var project = context.ProjectInformation.SourceProject;

        _logger.Information("    {CodeFix}", provider.GetType().FullName);

        var resolvedValues = ImmutableArray.CreateBuilder<ResolvedCodeFixTestResult>();

        foreach (var fixableDiagnostic in context.FinalDiagnostics
                                                 .Join(provider.FixableDiagnosticIds, z => z.Id, z => z, (d, _) => d))
        {
            if (project.GetDocument(fixableDiagnostic.Location.SourceTree) is { } document)
            {
                var cab = ImmutableArray.CreateBuilder<(Document Document, CodeAction CodeAction)>();
                var codeFixContext = new CodeFixContext(document, fixableDiagnostic, (a, _) => cab.Add(( document, a )), cancellationToken);
                await provider.RegisterCodeFixesAsync(codeFixContext);

                resolvedValues.Add(new(document, fixableDiagnostic, await CreateCodeActionTestResults(project, cab, cancellationToken)));
            }
            else if (fixableDiagnostic.Location.SourceTree is null)
            {
                foreach (var doc in project.Documents)
                {
                    var cab = ImmutableArray.CreateBuilder<(Document Document, CodeAction CodeAction)>();
                    var codeFixContext = new CodeFixContext(doc, fixableDiagnostic, (a, _) => cab.Add(( doc, a )), cancellationToken);
                    await provider.RegisterCodeFixesAsync(codeFixContext);
                    resolvedValues.Add(new(doc, fixableDiagnostic, await CreateCodeActionTestResults(project, cab, cancellationToken)));
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

    /// <summary>
    ///     Generate the code refactoring
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<CodeRefactoringTestResult> GenerateCodeRefactoring<T>(this GeneratorTestContext context, CancellationToken cancellationToken = default)
        where T : CodeRefactoringProvider, new()
    {
        return GenerateCodeRefactoring(context, typeof(T), cancellationToken);
    }

    /// <summary>
    ///     Generate the code refactoring
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<CodeRefactoringTestResult> GenerateCodeRefactoring<T>(this GeneratorTestContextBuilder builder, CancellationToken cancellationToken = default)
        where T : CodeRefactoringProvider, new()
    {
        return GenerateCodeRefactoring(builder, typeof(T), cancellationToken);
    }

    /// <summary>
    ///     Generate the code refactoring
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<CodeRefactoringTestResult> GenerateCodeRefactoring(this GeneratorTestContextBuilder builder, Type type, CancellationToken cancellationToken = default)
    {
        return builder.Build().GenerateCodeRefactoring(type, cancellationToken);
    }

    /// <summary>
    ///     Generate the code refactoring
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<CodeRefactoringTestResult> GenerateCodeRefactoring(this GeneratorTestContext context, Type type, CancellationToken cancellationToken = default)
    {
        var result = await context.IncludeRelatedType(type).GenerateAsync(cancellationToken);

        return result.CodeRefactoringResults.TryGetValue(type, out var analyzerResult)
            ? analyzerResult
            : throw new InvalidOperationException("Generator not found");
    }

    /// <summary>
    ///     Add the code refactoring to the results
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task<GeneratorTestResults> AddCodeRefactoring<T>(this GeneratorTestResults context, CancellationToken cancellationToken = default)
        where T : CodeRefactoringProvider, new()
    {
        return AddCodeRefactoring(context, new T(), cancellationToken);
    }

    /// <summary>
    ///     Add the code refactoring to the results
    /// </summary>
    /// <param name="context"></param>
    /// <param name="provider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<GeneratorTestResults> AddCodeRefactoring(this GeneratorTestResults context, CodeRefactoringProvider provider, CancellationToken cancellationToken = default)
    {
        var _logger = context.ProjectInformation.Logger;
        var project = context.ProjectInformation.SourceProject;

        var resolvedValues = ImmutableArray.CreateBuilder<ResolvedCodeRefactoringTestResult>();
        _logger.Information("    {CodeRefactoring}", provider.GetType().FullName);
        foreach (( var path, var mark ) in context.MarkedLocations)
        {
            var cab = ImmutableArray.CreateBuilder<(Document Document, CodeAction CodeAction)>();
            var document = project.Documents.Single(z => z.Name == path);
            var codeFixContext = new CodeRefactoringContext(document, mark.Location, a => cab.Add(( document, a )), cancellationToken);
            await provider.ComputeRefactoringsAsync(codeFixContext);
            resolvedValues.Add(new(document, mark, await CreateCodeActionTestResults(project, cab, cancellationToken)));
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
        ImmutableArray<(Document Document, CodeAction CodeAction)>.Builder cab,
        CancellationToken cancellationToken
    )
    {
        var originalSolution = project.Solution.Workspace.CurrentSolution;
        var results = ImmutableArray.CreateBuilder<CodeActionTestResult>();
        foreach (( var targetDocument, var codeAction ) in cab)
        {
            var operations = await codeAction.GetOperationsAsync(cancellationToken);
            foreach (var o in operations)
            {
                o.Apply(originalSolution.Workspace, cancellationToken);
            }

            var changedProject = project.Solution.Workspace.CurrentSolution.GetProject(project.Id);
            if (changedProject is null) continue;

            var documentChanges = changedProject.GetChanges(project);

            var documentTextChanges = ImmutableDictionary.CreateBuilder<string, ImmutableArray<TextChange>>();

            foreach (var changedDocument in documentChanges.GetChangedDocuments(true))
            {
                // ReSharper disable once NullableWarningSuppressionIsUsed
                var oldDocument = project.GetDocument(changedDocument)!;
                // ReSharper disable once NullableWarningSuppressionIsUsed
                var newDocument = changedProject.GetDocument(changedDocument)!;
                var textChanges = ( await newDocument.GetTextChangesAsync(oldDocument, cancellationToken) ).ToImmutableArray();

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
