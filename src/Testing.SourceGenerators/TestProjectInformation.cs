using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using Serilog;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Test project information
/// </summary>
/// <param name="Logger"></param>
/// <param name="SourceProject"></param>
/// <param name="Analyzers"></param>
/// <param name="SourceGenerators"></param>
/// <param name="IncrementalGenerators"></param>
/// <param name="CodeFixProviders"></param>
/// <param name="CodeRefactoringProviders"></param>
public record TestProjectInformation
(
    ILogger Logger,
    Project SourceProject,
    ImmutableDictionary<Type, DiagnosticAnalyzer> Analyzers,
    ImmutableDictionary<Type, ISourceGenerator> SourceGenerators,
    ImmutableDictionary<Type, IIncrementalGenerator> IncrementalGenerators,
    ImmutableDictionary<Type, CodeFixProvider> CodeFixProviders,
    ImmutableDictionary<Type, CodeRefactoringProvider> CodeRefactoringProviders
);