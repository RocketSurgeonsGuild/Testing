using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Some default customizers that allow for including or excluding various parts of the generator test results
/// </summary>
public static class Customizers
{
    public static GeneratorTestResultsCustomizer Default =
        ExcludeInputs + IncludeGlobalOptions + IncludeFileOptions + IncludeReferences;

    public static GeneratorTestResultsCustomizer Empty = (results, targets, data) => { };

    public static GeneratorTestResultsCustomizer Reset = (results, targets, data) =>
                                                         {
                                                             data.Clear();
                                                             RemoveTargets(targets, results);
                                                         };

    public static GeneratorTestResultsCustomizer IncludeInputs => static (results, targets, data) =>
                                                                  {
                                                                      RemoveTargets(targets, results);
                                                                      targets.AddRange(results.InputSyntaxTrees.Select(Selector));
                                                                      data["InputDiagnostics"] = results
                                                                                                .InputDiagnostics
                                                                                                .Where(s => s.Severity >= results.Severity)
                                                                                                .OrderDiagnosticResults();
                                                                      data["InputAdditionalTexts"] = results.InputAdditionalTexts;
                                                                  };

    public static GeneratorTestResultsCustomizer IncludeContextId => static (results, targets, data) => { data["ContextId"] = results.ContextId; };

    public static GeneratorTestResultsCustomizer ExcludeInputs => static (results, targets, data) =>
                                                                  {
                                                                      RemoveTargets(targets, results);
                                                                      data.Remove("InputDiagnostics");
                                                                      data.Remove("InputAdditionalTexts");
                                                                  };

    public static GeneratorTestResultsCustomizer IncludeParseOptions => (results, targets, data) => data["ParseOptions"] = new
    {
        results.ParseOptions.LanguageVersion,
        results.ParseOptions.DocumentationMode,
        results.ParseOptions.Kind,
        results.ParseOptions.Features,
        results.ParseOptions.PreprocessorSymbolNames,
    };

    public static GeneratorTestResultsCustomizer ExcludeParseOptions => static (results, targets, data) => data.Remove("ParseOptions");

    public static GeneratorTestResultsCustomizer IncludeGlobalOptions => static (results, targets, data) => data["GlobalOptions"] = results.GlobalOptions;

    public static GeneratorTestResultsCustomizer ExcludeGlobalOptions => static (results, targets, data) => data.Remove("GlobalOptions");

    public static GeneratorTestResultsCustomizer IncludeFileOptions => static (results, targets, data) => data["FileOptions"] = results.FileOptions;

    public static GeneratorTestResultsCustomizer ExcludeFileOptions => static (results, targets, data) => data.Remove("FileOptions");

    public static GeneratorTestResultsCustomizer IncludeReferences => static (results, targets, data) => data["References"] = results
                                                                         .FinalCompilation
                                                                         .References
                                                                         .Select(x => x.Display ?? "")
                                                                         .Select(Path.GetFileName)
                                                                         .OrderBy(z => z);

    public static GeneratorTestResultsCustomizer ExcludeReferences => static (results, targets, data) => data.Remove("References");

    internal static Target Selector(SyntaxTree source)
    {
        var hintPath = source.FilePath;
        var data = $@"//HintName: {hintPath.Replace("\\", "/")}
{source.GetText()}";
        return new("cs", data.Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase), Path.GetFileNameWithoutExtension(hintPath));
    }

    private static void RemoveTargets(List<Target> targets, GeneratorTestResults results)
    {
        targets.RemoveAll(
            z => results.InputSyntaxTrees.Any(
                x => z.Name.Equals(Path.GetFileNameWithoutExtension(x.FilePath))
            )
        );
    }
}
