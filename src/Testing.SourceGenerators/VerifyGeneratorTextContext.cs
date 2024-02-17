using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

public static class VerifyGeneratorTextContext
{
    public static void Initialize(
        bool includeInputs = true,
        bool includeOptions = true
    )
    {
        VerifyGeneratorTextContext.includeInputs = includeInputs;
        VerifyGeneratorTextContext.includeOptions = includeOptions;
        VerifySourceGenerators.Initialize();
        VerifierSettings.RegisterFileConverter<GeneratorTestResult>(Convert);
        VerifierSettings.RegisterFileConverter<GeneratorTestResults>(Convert);
        VerifierSettings.AddExtraSettings(
            serializer =>
            {
                var converters = serializer.Converters;
                converters.Add(new AdditionalTextConverter());
            }
        );
    }

    private static bool includeInputs;
    private static bool includeOptions;

    private static ConversionResult Convert(GeneratorTestResults target, IReadOnlyDictionary<string, object> context)
    {
        var targets = new List<Target>();
        if (includeInputs)
        {
            targets.AddRange(target.InputSyntaxTrees.Select(Selector));
        }

        foreach (var item in target.Results)
        {
            targets.AddRange(item.Value.SyntaxTrees.Select(Selector));
        }

        var data = new Dictionary<string, object>();
        if (includeInputs)
        {
            data["InputDiagnostics"] = target.InputDiagnostics.OrderDiagnosticResults();
            data["InputAdditionalTexts"] = target.InputAdditionalTexts;
        }

        if (includeOptions)
        {
            // start here
            data["ParseOptions"] = new
            {
                target.ParseOptions.LanguageVersion,
                target.ParseOptions.DocumentationMode,
                target.ParseOptions.Kind,
                target.ParseOptions.Features,
                target.ParseOptions.PreprocessorSymbolNames,
            };

            data["GlobalOptions"] = target.GlobalOptions;
            data["FileOptions"] = target.FileOptions;
            data["References"] = target
                                .FinalCompilation
                                .References
                                .Select(x => x.Display ?? "")
                                .Select(Path.GetFileName)
                                .OrderBy(z => z);
        }

        data["FinalDiagnostics"] = target.FinalDiagnostics.OrderDiagnosticResults();
        data["GeneratorDiagnostics"] = target.Results.ToDictionary(
            z => z.Key.FullName!,
            z => z.Value.Diagnostics.OrderDiagnosticResults()
        );

        return new(data, targets);
    }

    private static Target Selector(SyntaxTree source)
    {
        var hintPath = source.FilePath;
        var data = $@"//HintName: {hintPath.Replace("\\", "/")}
{source.GetText()}";
        return new("cs", data.Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase), Path.GetFileNameWithoutExtension(hintPath));
    }

    private static ConversionResult Convert(GeneratorTestResult target, IReadOnlyDictionary<string, object> context)
    {
        return new(new { target.Diagnostics, }, target.SyntaxTrees.Select(Selector));
    }

    private class AdditionalTextConverter :
        WriteOnlyJsonConverter<AdditionalText>
    {
        public override void Write(VerifyJsonWriter writer, AdditionalText value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Path, "Path");
            writer.WritePropertyName("Text");
            writer.WriteValue(value.GetText()?.ToString() ?? "");
            writer.WriteEndObject();
        }
    }
}