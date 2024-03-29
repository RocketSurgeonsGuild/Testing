using System.Collections.Immutable;
using System.Runtime.Loader;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     The context for running a set of generators against a set of sources
/// </summary>
[PublicAPI]
public record GeneratorTestContext
{
    private readonly ImmutableDictionary<string, MarkedLocation> _markedLocations;
    private readonly ImmutableArray<GeneratorTestResultsCustomizer> _customizers;

    internal GeneratorTestContext(
        Guid id,
        string projectName,
        ILogger logger,
        AssemblyLoadContext assemblyLoadContext,
        ImmutableHashSet<MetadataReference> metadataReferences,
        ImmutableHashSet<Type> relatedTypes,
        ImmutableArray<NamedSourceText> sources,
        ImmutableHashSet<string> ignoredFilePaths,
        ImmutableDictionary<string, ImmutableDictionary<string, string>> fileOptions,
        ImmutableDictionary<string, string> globalOptions,
        CSharpParseOptions parseOptions,
        ImmutableArray<AdditionalText> additionalTexts,
        ImmutableDictionary<string, MarkedLocation> markedLocations,
        DiagnosticSeverity? diagnosticSeverity,
        ImmutableArray<GeneratorTestResultsCustomizer> customizers
    )
    {
        Id = id;
        _markedLocations = markedLocations;
        _customizers = customizers;
        _logger = logger;
        _metadataReferences = metadataReferences;
        _relatedTypes = relatedTypes;
        _sources = sources;
        _ignoredFilePaths = ignoredFilePaths;
        _fileOptions = fileOptions;
        _globalOptions = globalOptions;
        _parseOptions = parseOptions;
        _additionalTexts = additionalTexts;
        _projectName = projectName;
        _diagnosticSeverity = diagnosticSeverity;
        AssemblyLoadContext = assemblyLoadContext;
    }

    /// <summary>
    ///     Gets the identifier for the context
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     The related assembly load context
    /// </summary>
    public AssemblyLoadContext AssemblyLoadContext { get; }

    internal ILogger _logger { get; init; }
    internal ImmutableHashSet<MetadataReference> _metadataReferences { get; init; }
    internal ImmutableHashSet<Type> _relatedTypes { get; init; }
    internal ImmutableArray<NamedSourceText> _sources { get; init; }
    internal ImmutableHashSet<string> _ignoredFilePaths { get; init; }
    internal ImmutableDictionary<string, ImmutableDictionary<string, string>> _fileOptions { get; init; }
    internal ImmutableDictionary<string, string> _globalOptions { get; init; }
    internal string _projectName { get; init; }
    internal CSharpParseOptions _parseOptions { get; init; }
    internal ImmutableArray<AdditionalText> _additionalTexts { get; init; }
    internal DiagnosticSeverity? _diagnosticSeverity { get; init; }

    /// <summary>
    ///     Generate and return the results of the generators
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<GeneratorTestResults> GenerateAsync()
    {
        _logger.LogInformation("Starting Generation for {SourceCount}", _sources.Length);
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("--- References --- {Count}", _metadataReferences.Count);
            foreach (var reference in _metadataReferences)
            {
                _logger.LogTrace("    Reference: {Name}", reference.Display);
            }
        }

        var project = GenerationHelpers.CreateProject(_projectName, _metadataReferences, _parseOptions, _sources, _additionalTexts);

        var compilation = (CSharpCompilation?)await project.GetCompilationAsync().ConfigureAwait(false);
        if (compilation is null) throw new InvalidOperationException("Could not compile the sources");

        var diagnostics = compilation.GetDiagnostics();
        if (_logger.IsEnabled(LogLevel.Trace) && diagnostics is { Length: > 0, })
        {
            _logger.LogTrace("--- Input Diagnostics --- {Count}", diagnostics.Length);
            foreach (var d in diagnostics)
            {
                _logger.LogTrace("    Reference: {Name}", d.ToString());
            }
        }

        var builder = ImmutableDictionary<Type, GeneratorTestResult>.Empty.ToBuilder();
        var analyzerBuilder = ImmutableDictionary<Type, AnalyzerTestResult>.Empty.ToBuilder();

        var inputCompilation = compilation;
        var inputDiagnostics = diagnostics;

        TestProjectInformation projectInformation;

        {
            var relatedInstances = _relatedTypes
                                  .Select(Activator.CreateInstance)
                                  .Where(z => z != null)
                                  .Select(z => z!)
                                  .ToImmutableArray()
                ;
            projectInformation = new(
                _logger,
                project,
                relatedInstances.OfType<DiagnosticAnalyzer>().ToImmutableDictionary(z => z.GetType()),
                relatedInstances.OfType<ISourceGenerator>().ToImmutableDictionary(z => z.GetType()),
                relatedInstances.OfType<IIncrementalGenerator>().ToImmutableDictionary(z => z.GetType()),
                relatedInstances.OfType<CodeFixProvider>().ToImmutableDictionary(z => z.GetType()),
                relatedInstances.OfType<CodeRefactoringProvider>().ToImmutableDictionary(z => z.GetType())
            );
        }

        var finalDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        foreach (var instance in projectInformation.IncrementalGenerators.Values.Concat(projectInformation.SourceGenerators.Values.Cast<object>()))
        {
            _logger.LogInformation("--- {Generator} ---", instance.GetType().FullName);
            var generators = new List<ISourceGenerator>
            {
                instance switch { IIncrementalGenerator i => i.AsSourceGenerator(), ISourceGenerator s => s, _ => throw new ArgumentOutOfRangeException(), },
            };
            var driver = CSharpGeneratorDriver.Create(generators, _additionalTexts, _parseOptions, new OptionsProvider(_fileOptions, _globalOptions), default);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);

            if (_logger.IsEnabled(LogLevel.Trace) && diagnostics is { Length: > 0, })
            {
                _logger.LogTrace("--- Diagnostics --- {Count}", diagnostics.Length);
                foreach (var d in diagnostics)
                {
                    _logger.LogTrace("    Reference: {Name}", d.ToString());
                }
            }

            var trees = outputCompilation
                       .SyntaxTrees
                       .Except(compilation.SyntaxTrees)
                       .ToImmutableArray();
            if (_logger.IsEnabled(LogLevel.Trace) && trees is { Length: > 0, })
            {
                _logger.LogTrace("--- Syntax Trees --- {Count}", trees.Length);
                foreach (var t in trees)
                {
                    _logger.LogTrace("    FilePath: {Name}", t.FilePath);
                    _logger.LogTrace("    Source:\n{Name}", ( await t.GetTextAsync().ConfigureAwait(false) ).ToString());
                }
            }

            inputCompilation = inputCompilation.AddSyntaxTrees(trees);
            finalDiagnostics.AddRange(diagnostics);

            builder.Add(
                instance.GetType(),
                new(
                    ( outputCompilation as CSharpCompilation )!,
                    diagnostics,
                    trees.Where(z => !_ignoredFilePaths.Any(x => z.FilePath.Contains(x))).ToImmutableArray()
                )
            );
        }

        var analyzers = projectInformation.Analyzers;
        finalDiagnostics.AddRange(inputCompilation.GetDiagnostics());

        AnalysisResult? analysisResult = null;
        if (analyzers.Count > 0)
        {
            _logger.LogInformation("--- Analyzers ---");
            foreach (var analyzer in analyzers)
            {
                _logger.LogInformation("    {Analyzer}", analyzer.Key.FullName);
            }

            var compilationWithAnalyzers = inputCompilation.WithAnalyzers(
                analyzers.Values.ToImmutableArray(),
                new AnalyzerOptions(_additionalTexts, new OptionsProvider(_fileOptions, _globalOptions))
            );

            analysisResult = await compilationWithAnalyzers.GetAnalysisResultAsync(CancellationToken.None);
            foreach (var analyzer in analyzers)
            {
                var analyzerResults = analysisResult.GetAllDiagnostics(analyzer.Value);
                analyzerBuilder.Add(analyzer.Key, new(analyzerResults));
            }

            finalDiagnostics.AddRange(analysisResult.GetAllDiagnostics());
        }

        var results = new GeneratorTestResults(
            projectInformation,
            compilation,
            inputDiagnostics,
            compilation.SyntaxTrees,
            _additionalTexts,
            _customizers,
            _diagnosticSeverity,
            _parseOptions,
            _globalOptions,
            _fileOptions,
            builder.ToImmutable(),
            analyzerBuilder.ToImmutable(),
            ImmutableDictionary<Type, CodeFixTestResult>.Empty,
            ImmutableDictionary<Type, CodeRefactoringTestResult>.Empty,
            _markedLocations,
            inputCompilation,
            finalDiagnostics.ToImmutable(),
            null!,
            null!
        );

        if (projectInformation.CodeFixProviders.Count > 0)
        {
            _logger.LogInformation("--- Code Fix Providers ---");
            foreach (var provider in projectInformation.CodeFixProviders.Values)
            {
                results = await results.AddCodeFix(provider);
            }
        }

        if (projectInformation.CodeRefactoringProviders.Count > 0)
        {
            _logger.LogInformation("--- Code Refactoring Providers ---");
            foreach (var provider in projectInformation.CodeRefactoringProviders.Values)
            {
                results = await results.AddCodeRefactoring(provider);
            }
        }

        var assemblyStream = Emit(inputCompilation);
        if (assemblyStream is { Length: > 0, })
            results = results with
            {
                MetadataReference = MetadataReference.CreateFromStream(new MemoryStream(assemblyStream)),
                Assembly = AssemblyLoadContext.LoadFromStream(new MemoryStream(assemblyStream)),
            };

        return results;
    }

    private byte[] Emit(CSharpCompilation compilation, string? outputName = null)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(
            stream,
            options: new(outputNameOverride: outputName ?? compilation.AssemblyName)
        );
        if (!emitResult.Success) return Array.Empty<byte>();

        return stream.ToArray();
    }
}