using System.Collections.Immutable;
using System.Composition.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal static class GenerationHelpers
{
    public static Project CreateProject(
        string projectName,
        IEnumerable<MetadataReference> metadataReferences,
        CSharpParseOptions parseOptions,
        CSharpCompilationOptions compilationOptions,
        ImmutableArray<NamedSourceText> sources,
        ImmutableArray<AdditionalText> additionalTexts
    )
    {
        var projectId = ProjectId.CreateNewId(projectName);


        var compositionConfiguration = new ContainerConfiguration().WithAssemblies(MefHostServices.DefaultAssemblies);
        var container = compositionConfiguration.CreateContainer();
        var host = new MefHostServices(container);


        var solution = new AdhocWorkspace(host)
                      .CurrentSolution
                      .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                      .WithProjectCompilationOptions(projectId, compilationOptions)
                      .WithProjectParseOptions(projectId, parseOptions)
                      .AddMetadataReferences(projectId, metadataReferences);

        var count = 0;
        foreach (var source in sources)
        {
            var newFileName = GetDocumentName(source.Name, count);

            var documentId = DocumentId.CreateNewId(projectId, newFileName);
            solution = solution.AddDocument(documentId, newFileName, source.SourceText);
            count++;
        }

        foreach (var item in additionalTexts)
        {
            if (item.GetText() is not { } source) continue;
            var documentId = DocumentId.CreateNewId(projectId, item.Path);
            solution = solution.AddAdditionalDocument(documentId, item.Path, source);
        }

        var project = solution.GetProject(projectId);

        return project is null
            ? throw new InvalidOperationException($"The ad hoc workspace does not contain a project with the id {projectId.Id}")
            : project;
    }

    /// <summary>
    ///     The document name a source gets in the project: its own name with ".cs" appended when missing, or Input{index}.cs.
    /// </summary>
    internal static string GetDocumentName(string? name, int index)
    {
        var fileName = name ?? DefaultFilePathPrefix + index + "." + CSharpDefaultFileExt;
        return fileName.EndsWith("." + CSharpDefaultFileExt, StringComparison.Ordinal) ? fileName : fileName + "." + CSharpDefaultFileExt;
    }

    internal const string DefaultFilePathPrefix = "Input";
    internal const string CSharpDefaultFileExt = "cs";
}
