using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     The context for running a set of generators against a set of sources
/// </summary>
public class GeneratorTestContext
{
    private readonly ILogger _logger;
    private readonly ImmutableHashSet<MetadataReference> _metadataReferences;
    private readonly ImmutableHashSet<Type> _generators;
    private readonly ImmutableArray<SourceText> _sources;
    private readonly ImmutableHashSet<string> _ignoredFilePaths;
    private readonly ImmutableDictionary<string, ImmutableDictionary<string, string>> _fileOptions;
    private readonly ImmutableDictionary<string, string> _globalOptions;
    private readonly string _projectName;
    private readonly CSharpParseOptions _parseOptions;
    private readonly ImmutableArray<AdditionalText> _additionalTexts;

    internal GeneratorTestContext(
        string projectName,
        ILogger logger,
        AssemblyLoadContext assemblyLoadContext,
        ImmutableHashSet<MetadataReference> metadataReferences,
        ImmutableHashSet<Type> generators,
        ImmutableArray<SourceText> sources,
        ImmutableHashSet<string> ignoredFilePaths,
        ImmutableDictionary<string, ImmutableDictionary<string, string>> fileOptions,
        ImmutableDictionary<string, string> globalOptions,
        CSharpParseOptions parseOptions,
        ImmutableArray<AdditionalText> additionalTexts
    )
    {
        _logger = logger;
        _metadataReferences = metadataReferences;
        _generators = generators;
        _sources = sources;
        _ignoredFilePaths = ignoredFilePaths;
        _fileOptions = fileOptions;
        _globalOptions = globalOptions;
        _parseOptions = parseOptions;
        _additionalTexts = additionalTexts;
        _projectName = projectName;
        AssemblyLoadContext = assemblyLoadContext;
    }

    /// <summary>
    ///     The related assembly load context
    /// </summary>
    public AssemblyLoadContext AssemblyLoadContext { get; }

    /// <summary>
    ///     Create a C# compilation from the sources with all the generators being run.
    /// </summary>
    /// <returns></returns>
    public CSharpCompilation Compile()
    {
        return GenerateAsync().ConfigureAwait(false).GetAwaiter().GetResult().FinalCompilation;
    }

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

        var project = GenerationHelpers.CreateProject(_projectName, _metadataReferences, _parseOptions, _sources.ToArray());

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

        var results = new GeneratorTestResults(
            compilation,
            diagnostics,
            compilation.SyntaxTrees,
            _additionalTexts,
            _parseOptions,
            _globalOptions,
            _fileOptions,
            ImmutableDictionary<Type, GeneratorTestResult>.Empty,
            null!,
            ImmutableArray<Diagnostic>.Empty,
            null!
        );

        var builder = ImmutableDictionary<Type, GeneratorTestResult>.Empty.ToBuilder();

        var inputCompilation = compilation;

        foreach (var generatorType in _generators)
        {
            _logger.LogInformation("--- {Generator} ---", generatorType.FullName);
            var generators = new List<ISourceGenerator>();
            var generator = Activator.CreateInstance(generatorType)!;
            switch (generator)
            {
                case IIncrementalGenerator g:
                    generators.Add(g.AsSourceGenerator());
                    break;
                case ISourceGenerator sg:
                    generators.Add(sg);
                    break;
            }

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

            builder.Add(
                generatorType,
                new(
                    ( outputCompilation as CSharpCompilation )!,
                    diagnostics,
                    trees.Where(z => !_ignoredFilePaths.Any(x => z.FilePath.Contains(x))).ToImmutableArray()
                )
            );
        }

        results = results with
        {
            FinalCompilation = inputCompilation,
            FinalDiagnostics = inputCompilation.GetDiagnostics(),
            Assembly = Emit(inputCompilation),
        };

        return results with { Results = builder.ToImmutable(), };
    }

    private Assembly? Emit(CSharpCompilation compilation, string? outputName = null)
    {
        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(
            stream,
            options: new(outputNameOverride: outputName ?? compilation.AssemblyName)
        );
        if (!emitResult.Success) return null;

        var data = stream.ToArray();

        using var assemblyStream = new MemoryStream(data);
        return AssemblyLoadContext.LoadFromStream(assemblyStream);
    }
}