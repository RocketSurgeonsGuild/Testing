using System.Collections.Immutable;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Serilog;
using Serilog.Events;
// ReSharper disable UseCollectionExpression

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     The context for running a set of generators against a set of sources
/// </summary>
/// <remarks>
///     Disposing the context unloads the assembly load context that <see cref="GeneratorTestContextBuilder.Build" /> created for it.
///     A load context passed to <see cref="GeneratorTestContextBuilder.WithAssemblyLoadContext" /> is left to its owner.
/// </remarks>
[PublicAPI]
public record GeneratorTestContext : IDisposable
{
    private readonly ImmutableDictionary<string, MarkedLocation> _markedLocations;
    private readonly ImmutableArray<GeneratorTestResultsCustomizer> _customizers;
    private readonly bool _ownsAssemblyLoadContext;

    internal GeneratorTestContext(
        string projectName,
        ILogger logger,
        AssemblyLoadContext assemblyLoadContext,
        bool ownsAssemblyLoadContext,
        ImmutableHashSet<MetadataReference> metadataReferences,
        ImmutableHashSet<string> referenceNames,
        ImmutableHashSet<Type> relatedTypes,
        ImmutableArray<NamedSourceText> sources,
        ImmutableHashSet<string> ignoredFilePaths,
        ImmutableDictionary<string, ImmutableDictionary<string, string>> fileOptions,
        ImmutableDictionary<string, string> globalOptions,
        CSharpParseOptions parseOptions,
        ImmutableArray<AdditionalText> additionalTexts,
        ImmutableDictionary<string, MarkedLocation> markedLocations,
        DiagnosticSeverity? diagnosticSeverity,
        ImmutableArray<GeneratorTestResultsCustomizer> customizers,
        CSharpCompilationOptions compilationOptions,
        GeneratorDriverOptions driverOptions
    )
    {
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
        _compilationOptions = compilationOptions;
        _driverOptions = driverOptions;
        AssemblyLoadContext = assemblyLoadContext;
        _ownsAssemblyLoadContext = ownsAssemblyLoadContext;

        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hasher.AppendData(Encoding.UTF8.GetBytes(projectName));
        foreach (var reference in referenceNames.OrderBy(z => z))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference));
        }

        foreach (var reference in relatedTypes.OrderBy(z => z.FullName))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference.FullName ?? ""));
        }

        foreach (var reference in sources.OrderBy(z => z.Name))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference.Name ?? ""));
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            reference.SourceText.Write(writer);
            hasher.AppendData(Encoding.UTF8.GetBytes(builder.Replace("\r\n", "\n").ToString()));
        }

        foreach (var reference in ignoredFilePaths.OrderBy(z => z))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference));
        }

        foreach (var reference in fileOptions.OrderBy(z => z.Key))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(reference.Key));
            foreach (var item in reference.Value.OrderBy(z => z.Key))
            {
                hasher.AppendData(Encoding.UTF8.GetBytes(item.Key));
                hasher.AppendData(Encoding.UTF8.GetBytes(item.Value));
            }
        }

        foreach (var item in globalOptions.OrderBy(z => z.Key))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Key));
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Value));
        }

        hasher.AppendData(Encoding.UTF8.GetBytes(parseOptions.Language));
        hasher.AppendData(Encoding.UTF8.GetBytes(parseOptions.LanguageVersion.ToString()));
        foreach (var item in parseOptions.Features.OrderBy(z => z.Key))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Key));
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Value));
        }

        foreach (var item in parseOptions.PreprocessorSymbolNames.OrderBy(z => z))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item));
        }

        foreach (var item in additionalTexts.OrderBy(z => z.Path))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Path));
            var text = item.GetText();
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            text?.Write(writer);
            hasher.AppendData(Encoding.UTF8.GetBytes(builder.Replace("\r\n", "\n").ToString()));
        }

        foreach (var item in markedLocations.OrderBy(z => z.Key))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Key));
            hasher.AppendData(Encoding.UTF8.GetBytes(item.Value.Location.ToString()));
            if (item.Value.Trigger is { } trigger)
            {
                hasher.AppendData(Encoding.UTF8.GetBytes(trigger.ToString() ?? ""));
            }
        }

        // Only non-default options are hashed, so contexts built without them keep the ids they had before compilation options existed.
        if (!compilationOptions.Equals(DefaultCompilationOptions))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(DescribeCompilationOptions(compilationOptions)));
        }

        Id = Convert.ToBase64String(hasher.GetCurrentHash());
    }

    /// <summary>
    ///     The related assembly load context
    /// </summary>
    public AssemblyLoadContext AssemblyLoadContext { get; }

    /// <summary>
    ///     Get the id of the context
    /// </summary>
    public string Id { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Unloads the assembly load context when this context created it
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _ownsAssemblyLoadContext && AssemblyLoadContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

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
    internal CSharpCompilationOptions _compilationOptions { get; init; }
    internal GeneratorDriverOptions _driverOptions { get; init; }

    /// <summary>
    ///     The compilation options used when none are configured.
    /// </summary>
    internal static readonly CSharpCompilationOptions DefaultCompilationOptions = new(OutputKind.DynamicallyLinkedLibrary);

    private static string DescribeCompilationOptions(CSharpCompilationOptions options)
    {
        var builder = new StringBuilder()
                     .Append(options.OutputKind).Append('|')
                     .Append(options.NullableContextOptions).Append('|')
                     .Append(options.AllowUnsafe).Append('|')
                     .Append(options.OptimizationLevel).Append('|')
                     .Append(options.Platform).Append('|')
                     .Append(options.CheckOverflow).Append('|')
                     .Append(options.WarningLevel).Append('|')
                     .Append(options.GeneralDiagnosticOption).Append('|')
                     .Append(options.ModuleName).Append('|')
                     .Append(options.MainTypeName).Append('|')
                     .Append(string.Join(",", options.Usings)).Append('|')
                     .Append(options.SyntaxTreeOptionsProvider?.GetType().FullName);
        foreach (var item in options.SpecificDiagnosticOptions.OrderBy(z => z.Key, StringComparer.Ordinal))
        {
            builder.Append('|').Append(item.Key).Append('=').Append(item.Value);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Generate and return the results of the generators
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<GeneratorTestResults> GenerateAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Starting Generation for {SourceCount}", _sources.Length);
        if (_logger.IsEnabled(LogEventLevel.Verbose))
        {
            _logger.Verbose("--- References --- {Count}", _metadataReferences.Count);
            foreach (var reference in _metadataReferences)
            {
                _logger.Verbose("    Reference: {Name}", reference.Display);
            }
        }

        var project = GenerationHelpers.CreateProject(_projectName, _metadataReferences, _parseOptions, _compilationOptions, _sources, _additionalTexts);

        var compilation = (CSharpCompilation?)await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Could not compile the sources");
        compilation = ApplyDiagnosticOptions(compilation);

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

        // Each generator gets its own driver over the original compilation, so no generator sees another's output.
        var optionsProvider = new OptionsProvider(_fileOptions, _globalOptions);
        var drivers = GeneratorsInOrder(projectInformation)
                     .Select(
                          z => (z.Type, (GeneratorDriver)CSharpGeneratorDriver.Create(
                              [
                                  z.Instance switch
                                  {
                                      IIncrementalGenerator i => i.AsSourceGenerator(),
                                      ISourceGenerator g => g,
                                      _ => throw new InvalidOperationException($"{z.Type.FullName} is not an incremental or source generator"),
                                  },
                              ],
                              _additionalTexts,
                              _parseOptions,
                              optionsProvider,
                              _driverOptions
                          ))
                      )
                     .ToList();

        var results = await RunAsync(projectInformation, compilation, drivers, cancellationToken).ConfigureAwait(false);

        if (projectInformation.CodeFixProviders.Count > 0)
        {
            _logger.Information("--- Code Fix Providers ---");
            foreach (var provider in projectInformation.CodeFixProviders.Values)
            {
                results = await results.AddCodeFix(provider, cancellationToken);
            }
        }

        if (projectInformation.CodeRefactoringProviders.Count > 0)
        {
            _logger.Information("--- Code Refactoring Providers ---");
            foreach (var provider in projectInformation.CodeRefactoringProviders.Values)
            {
                results = await results.AddCodeRefactoring(provider, cancellationToken);
            }
        }

        return Emit(results, true);
    }

    /// <summary>
    ///     Runs the drivers kept on <paramref name="previous" /> again on an edited compilation, then the analyzers and emit.
    /// </summary>
    internal async Task<GeneratorTestResults> RerunAsync(
        GeneratorTestResults previous,
        Func<CSharpCompilation, CSharpCompilation>? editCompilation,
        Func<GeneratorDriver, GeneratorDriver>? editDriver,
        CancellationToken cancellationToken
    )
    {
        var compilation = editCompilation?.Invoke(previous.InputCompilation) ?? previous.InputCompilation;
        var drivers = GeneratorsInOrder(previous.ProjectInformation)
                     .Select(
                          z =>
                          {
                              var driver = previous.Results.TryGetValue(z.Type, out var result) && result.Driver is { } d
                                  ? d
                                  : throw new InvalidOperationException($"The results for {z.Type.FullName} have no generator driver to run again.");
                              return (z.Type, editDriver?.Invoke(driver) ?? driver);
                          }
                      )
                     .ToList();

        var results = await RunAsync(previous.ProjectInformation, compilation, drivers, cancellationToken).ConfigureAwait(false);
        // The first run already loaded an assembly with this name into the load context, so a rerun only emits.
        return Emit(results, false);
    }

    private static IEnumerable<(Type Type, object Instance)> GeneratorsInOrder(TestProjectInformation projectInformation) =>
        projectInformation
           .IncrementalGenerators.Select(z => (z.Key, (object)z.Value))
           .Concat(projectInformation.SourceGenerators.Select(z => (z.Key, (object)z.Value)));

    private CSharpCompilation ApplyDiagnosticOptions(CSharpCompilation compilation)
    {
        // The workspace builds its own syntax tree options, so the severities from analyzer config options are applied to the compilation it returns.
        var provider = _compilationOptions.SyntaxTreeOptionsProvider
                                           ?? AnalyzerConfigSyntaxTreeOptionsProvider.Create(_fileOptions, _globalOptions);
        return provider is null ? compilation : compilation.WithOptions(compilation.Options.WithSyntaxTreeOptionsProvider(provider));
    }

    private async Task<GeneratorTestResults> RunAsync(
        TestProjectInformation projectInformation,
        CSharpCompilation compilation,
        IEnumerable<(Type Type, GeneratorDriver Driver)> drivers,
        CancellationToken cancellationToken
    )
    {
        var diagnostics = compilation.GetDiagnostics(cancellationToken);
        LogDiagnostics("--- Input Diagnostics --- {Count}", diagnostics);

        var builder = ImmutableDictionary<Type, GeneratorTestResult>.Empty.ToBuilder();

        var inputCompilation = compilation;
        var inputDiagnostics = diagnostics;

        var finalDiagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        foreach ((var type, var driver) in drivers)
        {
            _logger.Information("--- {Generator} ---", type.FullName);
            var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics, cancellationToken);
            LogDiagnostics("--- Diagnostics --- {Count}", diagnostics);

            var trees = outputCompilation
                       .SyntaxTrees
                       .Except(compilation.SyntaxTrees)
                       .ToImmutableArray();
            await LogSyntaxTreesAsync(trees, cancellationToken).ConfigureAwait(false);

            inputCompilation = inputCompilation.AddSyntaxTrees(trees);
            finalDiagnostics.AddRange(diagnostics);

            builder.Add(
                type,
                new(
                    ( outputCompilation as CSharpCompilation )!,
                    diagnostics,
                    [.. trees.Where(z => !_ignoredFilePaths.Any(x => z.FilePath.Contains(x))),]
                )
                {
                    Driver = updatedDriver,
                    DriverRunResult = updatedDriver.GetRunResult(),
                }
            );
        }

        finalDiagnostics.AddRange(inputCompilation.GetDiagnostics(cancellationToken));
        var analyzerResults = await RunAnalyzersAsync(projectInformation.Analyzers, inputCompilation, finalDiagnostics, cancellationToken).ConfigureAwait(false);

        return new(
            Id,
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
            analyzerResults,
            [],
            [],
            _markedLocations,
            inputCompilation,
            finalDiagnostics.ToImmutable(),
            null,
            null
        )
        {
            Context = this,
        };
    }

    private async Task<ImmutableDictionary<Type, AnalyzerTestResult>> RunAnalyzersAsync(
        ImmutableDictionary<Type, DiagnosticAnalyzer> analyzers,
        CSharpCompilation compilation,
        ImmutableArray<Diagnostic>.Builder finalDiagnostics,
        CancellationToken cancellationToken
    )
    {
        if (analyzers.Count == 0)
        {
            return [];
        }

        _logger.Information("--- Analyzers ---");
        foreach (var analyzer in analyzers)
        {
            _logger.Information("    {Analyzer}", analyzer.Key.FullName);
        }

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            [.. analyzers.Values,],
            new AnalyzerOptions(_additionalTexts, new OptionsProvider(_fileOptions, _globalOptions))
        );

        var analysisResult = await compilationWithAnalyzers.GetAnalysisResultAsync(cancellationToken).ConfigureAwait(false);
        finalDiagnostics.AddRange(analysisResult.GetAllDiagnostics());
        return analyzers.ToImmutableDictionary(z => z.Key, z => new AnalyzerTestResult(analysisResult.GetAllDiagnostics(z.Value)));
    }

    private void LogDiagnostics(string messageTemplate, ImmutableArray<Diagnostic> diagnostics)
    {
        if (!_logger.IsEnabled(LogEventLevel.Verbose) || diagnostics.IsEmpty)
        {
            return;
        }

        _logger.Verbose(messageTemplate, diagnostics.Length);
        foreach (var d in diagnostics)
        {
            _logger.Verbose("    Reference: {Name}", d.ToString());
        }
    }

    private async Task LogSyntaxTreesAsync(ImmutableArray<SyntaxTree> trees, CancellationToken cancellationToken)
    {
        if (!_logger.IsEnabled(LogEventLevel.Verbose) || trees.IsEmpty)
        {
            return;
        }

        _logger.Verbose("--- Syntax Trees --- {Count}", trees.Length);
        foreach (var t in trees)
        {
            _logger.Verbose("    FilePath: {Name}", t.FilePath);
            _logger.Verbose("    Source:\n{Name}", ( await t.GetTextAsync(cancellationToken).ConfigureAwait(false) ).ToString());
        }
    }

    private GeneratorTestResults Emit(GeneratorTestResults results, bool loadAssembly)
    {
        var compilation = results.FinalCompilation;
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream, options: new(outputNameOverride: compilation.AssemblyName));
        if (!emitResult.Success)
        {
            _logger.Warning("--- Emit failed --- {Count}", emitResult.Diagnostics.Length);
            foreach (var d in emitResult.Diagnostics)
            {
                _logger.Warning("    {Diagnostic}", d.ToString());
            }

            return results with { EmitResult = emitResult, };
        }

        var image = stream.ToArray();
        return results with
        {
            EmitResult = emitResult,
            Image = [.. image,],
            MetadataReference = MetadataReference.CreateFromStream(new MemoryStream(image)),
            Assembly = loadAssembly ? GeneratedAssemblyLocation.LoadFromStream(AssemblyLoadContext, Id, image) : null,
        };
    }
}
