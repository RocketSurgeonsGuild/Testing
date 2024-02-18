using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

public static class VerifyGeneratorTextContext
{
    public static void Initialize(
        bool includeInputs = true,
        bool includeOptions = true,
        DiagnosticSeverity diagnosticSeverityFilter = DiagnosticSeverity.Warning
    )
    {
        VerifyGeneratorTextContext.includeInputs = includeInputs;
        VerifyGeneratorTextContext.includeOptions = includeOptions;
        VerifyGeneratorTextContext.diagnosticSeverityFilter = diagnosticSeverityFilter;
        VerifySourceGenerators.Initialize();
        VerifierSettings.RegisterFileConverter<GeneratorTestResult>(Convert);
        VerifierSettings.RegisterFileConverter<GeneratorTestResults>(Convert);
        VerifierSettings.AddExtraSettings(
            serializer =>
            {
                var converters = serializer.Converters;
                converters.Add(new AdditionalTextConverter());

                serializer.Converters.Remove(serializer.Converters.Find(z => z.GetType().Name == "LocationConverter")!);
                serializer.Converters.Add(new LocationConverter());

                serializer.Converters.Remove(serializer.Converters.Find(z => z.GetType().Name == "DiagnosticConverter")!);
                serializer.Converters.Add(new DiagnosticConverter());
            }
        );
    }

    private static bool includeInputs;
    private static bool includeOptions;
    private static DiagnosticSeverity diagnosticSeverityFilter;

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
            data["InputDiagnostics"] = target.InputDiagnostics.OrderDiagnosticResults(diagnosticSeverityFilter);
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

        data["FinalDiagnostics"] = target.FinalDiagnostics.OrderDiagnosticResults(diagnosticSeverityFilter);
        data["GeneratorDiagnostics"] = target.Results.ToDictionary(
            z => z.Key.FullName!,
            z => z.Value.Diagnostics.OrderDiagnosticResults(diagnosticSeverityFilter)
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

    private class DiagnosticConverter : WriteOnlyJsonConverter<Diagnostic>
    {
        public override void Write(VerifyJsonWriter writer, Diagnostic value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Id, "Id");
            var descriptor = value.Descriptor;
            writer.WriteMember(value, descriptor.Title.ToString(), "Title");
            writer.WriteMember(value, value.Severity.ToString(), "Severity");
            writer.WriteMember(value, value.WarningLevel, "WarningLevel");
            writer.WriteMember(value, value.Location.GetMappedLineSpan().ToString().Replace("\\", "/"), "Location");
            var description = descriptor.Description.ToString();
            if (!string.IsNullOrWhiteSpace(description))
            {
                writer.WriteMember(value, description, "Description");
            }

            var help = descriptor.HelpLinkUri;
            if (!string.IsNullOrWhiteSpace(help))
            {
                writer.WriteMember(value, help, "HelpLink");
            }

            writer.WriteMember(value, descriptor.MessageFormat.ToString(), "MessageFormat");
            writer.WriteMember(value, value.GetMessage(), "Message");
            writer.WriteMember(value, descriptor.Category, "Category");
            writer.WriteMember(value, descriptor.CustomTags, "CustomTags");
            writer.WriteEndObject();
        }
    }

    private class LocationConverter :
        WriteOnlyJsonConverter<Location>
    {
        public override void Write(VerifyJsonWriter writer, Location value)
        {
            writer.WriteValue(value.GetMappedLineSpan().ToString().Replace("\\", "/"));
        }
    }
}