﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

/// <summary>
///     Configures verify for use with testing roslyn generators and analyzers
/// </summary>
public static class VerifyGeneratorTextContext
{
    /// <summary>
    ///     Initializes the context
    /// </summary>
    /// <param name="includeInputs"></param>
    /// <param name="includeOptions"></param>
    /// <param name="diagnosticSeverityFilter"></param>
    [Obsolete("Use Initialize with customizer methods instead")]
    public static void Initialize(
        // ReSharper disable ParameterHidesMember
        bool includeInputs = false,
        bool includeOptions = true,
        DiagnosticSeverity? diagnosticSeverityFilter = null
        // ReSharper enable ParameterHidesMember
    )
    {
        Initialize(
            diagnosticSeverityFilter,
            includeInputs ? Customizers.IncludeInputs : Customizers.ExcludeInputs,
            includeOptions ? Customizers.IncludeParseOptions : Customizers.ExcludeParseOptions,
            includeOptions ? Customizers.IncludeGlobalOptions : Customizers.ExcludeGlobalOptions,
            includeOptions ? Customizers.IncludeFileOptions : Customizers.ExcludeFileOptions,
            includeOptions ? Customizers.IncludeReferences : Customizers.ExcludeReferences
        );
    }

    /// <summary>
    ///     Initializes the context
    /// </summary>
    /// <param name="customizers"></param>
    public static void Initialize(params GeneratorTestResultsCustomizer[] customizers)
    {
        Initialize(null, customizers);
    }

    /// <summary>
    ///     Initializes the context
    /// </summary>
    /// <param name="customizers"></param>
    public static void Initialize(DiagnosticSeverity? diagnosticSeverityFilter, params GeneratorTestResultsCustomizer[] customizers)
    {
        if (customizers is [])
        {
            customizers = [Customizers.Default,];
        }

        VerifyGeneratorTextContext.diagnosticSeverityFilter = diagnosticSeverityFilter;
        VerifySourceGenerators.Initialize();
        VerifierSettings.RegisterFileConverter<GeneratorTestResult>(Convert);
        VerifierSettings.RegisterFileConverter(Convert(customizers));
        VerifierSettings.RegisterFileConverter<CompletionTestResult>(Convert);
        VerifierSettings.RegisterFileConverter<CodeRefactoringTestResult>(Convert);
        VerifierSettings.RegisterFileConverter<CodeFixTestResult>(Convert);
        VerifierSettings.RegisterFileConverter<AnalyzerTestResult>(Convert);
        VerifierSettings.AddExtraSettings(
            serializer =>
            {
                var converters = serializer.Converters;
                converters.Add(new AdditionalTextConverter());
                converters.Add(new CodeActionTestResultConverter());
                converters.Add(new CompletionTestResultConverter());
                converters.Add(new CodeRefactoringTestResultConverter());
                converters.Add(new CodeFixTestResultConverter());
                converters.Add(new GeneratorTestResultConverter());
                converters.Add(new AnalyzerTestResultConverter());
                converters.Add(new CodeActionConverter());
                converters.Add(new ResolvedCodeRefactoringTestResultConverter());
                converters.Add(new CompletionListTestResultConverter());
                converters.Add(new CompletionItemTestResultConverter());
                converters.Add(new ResolvedCodeFixTestResultConverter());
                converters.Add(new TextChangeConverter());

                serializer.Converters.Remove(serializer.Converters.Find(z => z.GetType().Name == "LocationConverter")!);
                serializer.Converters.Add(new LocationConverter());

                serializer.Converters.Remove(serializer.Converters.Find(z => z.GetType().Name == "DiagnosticConverter")!);
                serializer.Converters.Add(new DiagnosticConverter());
            }
        );
    }

    internal static DiagnosticSeverity? diagnosticSeverityFilter;

    private static Conversion<GeneratorTestResults> Convert(IEnumerable<GeneratorTestResultsCustomizer> customizers)
    {
        var customizer = customizers.Aggregate(Customizers.Empty, generatorTestResultsCustomizer);

        return (target, context) =>
               {
                   var targets = new List<Target>();
                   var data = new Dictionary<string, object>();
                   executeDelegate(target.Customizers.Aggregate(customizer, generatorTestResultsCustomizer))(target, targets, data);

                   foreach (var item in target.Results)
                   {
                       targets.AddRange(item.Value.SyntaxTrees.Select(Customizers.Selector));
                   }

                   data["FinalDiagnostics"] = target.FinalDiagnostics.Where(s => s.Severity >= target.Severity).OrderDiagnosticResults();
                   data["GeneratorDiagnostics"] = target.Results.ToDictionary(z => z.Key.FullName!, z => z.Value.Diagnostics.OrderDiagnosticResults());
                   data["AnalyzerDiagnostics"] = target.AnalyzerResults.ToDictionary(z => z.Key.FullName!, z => z.Value.Diagnostics.OrderDiagnosticResults());

                   if (target.CodeFixResults.Count > 0)
                   {
                       var results = new Dictionary<string, object>();
                       foreach (var result in target.CodeFixResults)
                       {
                           var converted = Convert(result.Value, context);
                           results[result.Key.FullName!] = converted.Info!;
                           targets.AddRange(converted.Targets);
                       }

                       data["CodeFixes"] = results;
                   }

                   if (target.CodeRefactoringResults.Count > 0)
                   {
                       var results = new Dictionary<string, object>();
                       foreach (var result in target.CodeRefactoringResults)
                       {
                           var converted = Convert(result.Value, context);
                           results[result.Key.FullName!] = converted.Info!;
                           targets.AddRange(converted.Targets);
                       }

                       data["CodeRefactorings"] = results;
                   }

                   return new(data, targets);
               };

        static GeneratorTestResultsCustomizer generatorTestResultsCustomizer(GeneratorTestResultsCustomizer seed, GeneratorTestResultsCustomizer value)
        {
            return seed + value;
        }

        static GeneratorTestResultsCustomizer executeDelegate(GeneratorTestResultsCustomizer customizer)
        {
            if (customizer.GetInvocationList() is { Length: > 0, } methods)
            {
                return (results, target, data) =>
                       {
                           foreach (var method in methods.OfType<GeneratorTestResultsCustomizer>())
                           {
                               method.Invoke(results, target, data);
                           }
                       };
            }

            return customizer;
        }
    }

    private static ConversionResult Convert(GeneratorTestResult target, IReadOnlyDictionary<string, object> context)
    {
        return new(new { target.Diagnostics, }, target.SyntaxTrees.Select(Customizers.Selector));
    }

    private static ConversionResult Convert(AnalyzerTestResult target, IReadOnlyDictionary<string, object> context)
    {
        return new(new { target.Diagnostics, }, Enumerable.Empty<Target>());
    }

    private static ConversionResult Convert(CompletionTestResult target, IReadOnlyDictionary<string, object> context)
    {
        return new(new { target.CompletionLists, }, Enumerable.Empty<Target>());
    }

    private static ConversionResult Convert(CodeRefactoringTestResult target, IReadOnlyDictionary<string, object> context)
    {
        var targets = new List<Target>();
        var data = new List<object>();
        foreach (var result in target.ResolvedFixes)
        {
            var documentData = new Dictionary<string, object>();
            var codeActions = new List<object>();
            data.Add(documentData);
            documentData["Document"] = result.Document.Name;
            documentData["Location"] = result.MarkedLocation.Location.ToString();
            if (result.MarkedLocation.Trigger is { })
            {
                documentData["TriggerKind"] = result.MarkedLocation.Trigger.Value.Kind.ToString();
                documentData["TriggerCharacter"] = result.MarkedLocation.Trigger.Value.Character.ToString();
            }

            documentData["CodeActions"] = codeActions;
            foreach (var action in result.CodeActions)
            {
                codeActions.Add(new { action.CodeAction.Title, action.CodeAction.Tags, action.TextChanges, });
                foreach (var changedDocumentId in action.Changes.GetChangedDocuments(true))
                {
                    action.Changes.NewProject.GetDocument(changedDocumentId)!.GetTextAsync().GetAwaiter().GetResult();
                    targets.Add(selector(action.Changes.NewProject.GetDocument(changedDocumentId)!, action.CodeAction.Title.Replace(" ", "_")));
                }

                foreach (var addedDocumentId in action.Changes.GetAddedDocuments())
                {
                    action.Changes.NewProject.GetDocument(addedDocumentId)!.GetTextAsync().GetAwaiter().GetResult();
                    targets.Add(selector(action.Changes.NewProject.GetDocument(addedDocumentId)!, action.CodeAction.Title.Replace(" ", "_")));
                }
            }
        }

        return new(data, targets);

        static Target selector(Document source, string additionalHintPath)
        {
            var hintPath = source.FilePath;
            var data = $@"//HintName: {hintPath?.Replace("\\", "/")}
{source.GetTextAsync().GetAwaiter().GetResult()}";
            return new(
                "cs",
                data.Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase),
                Path.GetFileNameWithoutExtension(hintPath) + "_" + additionalHintPath
            );
        }
    }

    private static ConversionResult Convert(CodeFixTestResult target, IReadOnlyDictionary<string, object> context)
    {
        var targets = new List<Target>();
        var data = new List<object>();
        foreach (var result in target.ResolvedFixes)
        {
            var documentData = new Dictionary<string, object>();
            var codeActions = new List<object>();
            data.Add(documentData);
            documentData["Document"] = result.Document.Name;
            documentData["Diagnostic"] = result.Diagnostic;
            documentData["CodeActions"] = codeActions;
            foreach (var action in result.CodeActions)
            {
                codeActions.Add(new { action.CodeAction.Title, action.CodeAction.Tags, action.TextChanges, });
                foreach (var changedDocumentId in action.Changes.GetChangedDocuments(true))
                {
                    action.Changes.NewProject.GetDocument(changedDocumentId)!.GetTextAsync().GetAwaiter().GetResult();
                    targets.Add(selector(action.Changes.NewProject.GetDocument(changedDocumentId)!, action.CodeAction.Title.Replace(" ", "_")));
                }

                foreach (var addedDocumentId in action.Changes.GetAddedDocuments())
                {
                    action.Changes.NewProject.GetDocument(addedDocumentId)!.GetTextAsync().GetAwaiter().GetResult();
                    targets.Add(selector(action.Changes.NewProject.GetDocument(addedDocumentId)!, action.CodeAction.Title.Replace(" ", "_")));
                }
            }
        }

        return new(data, targets);

        static Target selector(Document source, string additionalHintPath)
        {
            var hintPath = source.FilePath;
            var data = $@"//HintName: {hintPath?.Replace("\\", "/")}
{source.GetTextAsync().GetAwaiter().GetResult()}";
            return new(
                "cs",
                data.Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase),
                Path.GetFileNameWithoutExtension(hintPath) + "_" + additionalHintPath
            );
        }
    }

    private class CodeFixTestResultConverter : WriteOnlyJsonConverter<CodeFixTestResult>
    {
        public override void Write(VerifyJsonWriter writer, CodeFixTestResult value)
        {
            writer.WriteStartArray();
            foreach (var item in value.ResolvedFixes)
            {
                writer.Serializer.Serialize(writer, item);
            }

            writer.WriteStartObject();
        }
    }


    private class ResolvedCodeFixTestResultConverter : WriteOnlyJsonConverter<ResolvedCodeFixTestResult>
    {
        public override void Write(VerifyJsonWriter writer, ResolvedCodeFixTestResult value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Document.FilePath, nameof(value.Document));
            writer.WriteMember(value, value.Diagnostic, nameof(value.Diagnostic));
            writer.WriteMember(value, value.CodeActions, nameof(value.CodeActions));
            writer.WriteEndObject();
        }
    }

    private class CompletionTestResultConverter : WriteOnlyJsonConverter<CompletionTestResult>
    {
        public override void Write(VerifyJsonWriter writer, CompletionTestResult value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.CompletionLists, nameof(value.CompletionLists));
            writer.WriteEndObject();
        }
    }

    private class CompletionListTestResultConverter : WriteOnlyJsonConverter<CompletionListTestResult>
    {
        public override void Write(VerifyJsonWriter writer, CompletionListTestResult value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Document.Name, nameof(value.Document));
            writer.WriteMember(value, value.MarkedLocation, nameof(value.MarkedLocation));
            writer.WriteMember(value, value.SuggestionModeItem, nameof(value.SuggestionModeItem));
            writer.WriteMember(value, value.Span, nameof(value.Span));
            writer.WriteMember(value, value.Items, nameof(value.Items));

            writer.WriteEndObject();
        }
    }

    private class CompletionItemTestResultConverter : WriteOnlyJsonConverter<CompletionItemTestResult>
    {
        public override void Write(VerifyJsonWriter writer, CompletionItemTestResult value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Item, nameof(value.Item));
            writer.WriteMember(value, value.Description, nameof(value.Description));
            writer.WriteMember(value, value.Change, nameof(value.Change));
            writer.WriteEndObject();
        }
    }

    private class CodeRefactoringTestResultConverter : WriteOnlyJsonConverter<CodeRefactoringTestResult>
    {
        public override void Write(VerifyJsonWriter writer, CodeRefactoringTestResult value)
        {
            writer.WriteStartArray();
            foreach (var item in value.ResolvedFixes)
            {
                writer.Serializer.Serialize(writer, item);
            }

            writer.WriteStartArray();
        }
    }

    private class ResolvedCodeRefactoringTestResultConverter : WriteOnlyJsonConverter<ResolvedCodeRefactoringTestResult>
    {
        public override void Write(VerifyJsonWriter writer, ResolvedCodeRefactoringTestResult value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Document.FilePath, nameof(value.Document));
            writer.WriteMember(value, value.MarkedLocation.Location.ToString(), nameof(value.MarkedLocation.Location));
            if (value.MarkedLocation.Trigger is { })
            {
                writer.WriteMember(value, value.MarkedLocation.Trigger.Value.Kind, "TriggerKind");
                writer.WriteMember(value, value.MarkedLocation.Trigger.Value.Character, "TriggerCharacter");
            }

            writer.WriteMember(value, value.CodeActions, nameof(value.CodeActions));
            writer.WriteEndObject();
        }
    }

    private class GeneratorTestResultConverter : WriteOnlyJsonConverter<GeneratorTestResult>
    {
        public override void Write(VerifyJsonWriter writer, GeneratorTestResult value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Diagnostics, nameof(value.Diagnostics));
            writer.WriteMember(value, value.SyntaxTrees, nameof(value.SyntaxTrees));
            writer.WriteEndObject();
        }
    }

    private class AnalyzerTestResultConverter : WriteOnlyJsonConverter<AnalyzerTestResult>
    {
        public override void Write(VerifyJsonWriter writer, AnalyzerTestResult value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Diagnostics, nameof(value.Diagnostics));
            writer.WriteEndObject();
        }
    }

    private class CodeActionTestResultConverter :
        WriteOnlyJsonConverter<CodeActionTestResult>
    {
        public override void Write(VerifyJsonWriter writer, CodeActionTestResult value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.TextChanges, nameof(value.TextChanges));
            writer.WriteMember(value, value.CodeAction, nameof(value.CodeAction));
            writer.WriteEndObject();
        }
    }

    private class TextChangeConverter :
        WriteOnlyJsonConverter<TextChange>
    {
        public override void Write(VerifyJsonWriter writer, TextChange value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Span.Length, nameof(value.Span.Length));
            writer.WriteMember(value, value.NewText, nameof(value.NewText));
            writer.WriteEndObject();
        }
    }

    private class CodeActionConverter :
        WriteOnlyJsonConverter<CodeAction>
    {
        public override void Write(VerifyJsonWriter writer, CodeAction value)
        {
            writer.WriteStartObject();
            writer.WriteMember(value, value.Title, nameof(value.Title));
            writer.WriteMember(value, value.Tags, nameof(value.Tags));
            writer.WriteEndObject();
        }
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
            if (!string.IsNullOrWhiteSpace(description)) writer.WriteMember(value, description, "Description");

            var help = descriptor.HelpLinkUri;
            if (!string.IsNullOrWhiteSpace(help)) writer.WriteMember(value, help, "HelpLink");

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