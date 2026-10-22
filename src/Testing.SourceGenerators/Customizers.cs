using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Some default customizers that allow for including or excluding various parts of the generator test results
/// </summary>
public static class Customizers
{
    /// <summary>
    ///     The default customizer: excludes inputs and includes global options, file options and references.
    /// </summary>
    public static readonly GeneratorTestResultsCustomizer Default =
        ExcludeInputs + IncludeGlobalOptions + IncludeFileOptions + IncludeReferences;

    /// <summary>
    ///     A customizer that changes nothing.
    /// </summary>
    public static readonly GeneratorTestResultsCustomizer Empty = (results, targets, data) => { };

    /// <summary>
    ///     A customizer that clears all collected data and removes the input targets.
    /// </summary>
    public static readonly GeneratorTestResultsCustomizer Reset = (results, targets, data) =>
                                                         {
                                                             data.Clear();
                                                             RemoveTargets(targets, results);
                                                         };

    /// <summary>
    ///     Includes the input syntax trees, input diagnostics and additional texts.
    /// </summary>
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

    /// <summary>
    ///     Includes the generator test context id.
    /// </summary>
    public static GeneratorTestResultsCustomizer IncludeContextId => static (results, targets, data) => { data["ContextId"] = results.ContextId; };

    /// <summary>
    ///     Excludes the input syntax trees, input diagnostics and additional texts.
    /// </summary>
    public static GeneratorTestResultsCustomizer ExcludeInputs => static (results, targets, data) =>
                                                                  {
                                                                      RemoveTargets(targets, results);
                                                                      data.Remove("InputDiagnostics");
                                                                      data.Remove("InputAdditionalTexts");
                                                                  };

    /// <summary>
    ///     Includes the parse options.
    /// </summary>
    public static GeneratorTestResultsCustomizer IncludeParseOptions => (results, targets, data) => data["ParseOptions"] = new
    {
        results.ParseOptions.DocumentationMode,
        results.ParseOptions.Kind,
        results.ParseOptions.Features,
        results.ParseOptions.PreprocessorSymbolNames,
    };

    /// <summary>
    ///     Excludes the parse options.
    /// </summary>
    public static GeneratorTestResultsCustomizer ExcludeParseOptions => static (results, targets, data) => data.Remove("ParseOptions");

    /// <summary>
    ///     Includes the global analyzer config options.
    /// </summary>
    public static GeneratorTestResultsCustomizer IncludeGlobalOptions => static (results, targets, data) => data["GlobalOptions"] = results.GlobalOptions;

    /// <summary>
    ///     Excludes the global analyzer config options.
    /// </summary>
    public static GeneratorTestResultsCustomizer ExcludeGlobalOptions => static (results, targets, data) => data.Remove("GlobalOptions");

    /// <summary>
    ///     Includes the per-file analyzer config options.
    /// </summary>
    public static GeneratorTestResultsCustomizer IncludeFileOptions => static (results, targets, data) => data["FileOptions"] = results.FileOptions;

    /// <summary>
    ///     Excludes the per-file analyzer config options.
    /// </summary>
    public static GeneratorTestResultsCustomizer ExcludeFileOptions => static (results, targets, data) => data.Remove("FileOptions");

    /// <summary>
    ///     Includes the file names of the final compilation's references.
    /// </summary>
    public static GeneratorTestResultsCustomizer IncludeReferences => static (results, targets, data) => data["References"] = results
                                                                         .FinalCompilation
                                                                         .References
                                                                         .Select(x => x.Display ?? "")
                                                                         .Select(Path.GetFileName)
                                                                         .OrderBy(z => z);

    /// <summary>
    ///     Excludes the compilation references.
    /// </summary>
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
                x => string.Equals(z.Name, Path.GetFileNameWithoutExtension(x.FilePath), StringComparison.Ordinal)
            )
        );
    }
}
