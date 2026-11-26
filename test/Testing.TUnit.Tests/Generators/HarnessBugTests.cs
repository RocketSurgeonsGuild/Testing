using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests.Generators;

/// <summary>
///     Regression tests for harness bugs: each one failed before its fix.
/// </summary>
public class HarnessBugTests() : LoggerTest<TestRecord>(TestRecord.Create())
{
    [Test]
    public async Task Snapshot_Includes_All_Diagnostics_When_Severity_Is_Null()
    {
        // null is documented as "all", so this must match the Hidden snapshot.
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithLogger(Logger)
                     .WithDiagnosticSeverity(null)
                     .WithGenerator<MyDiagnosticGenerator>()
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Test]
    public async Task Each_Code_Action_Has_Its_Own_Changes()
    {
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .AddSources("public class A { }")
                           .WithAnalyzer<TestAnalyzer>()
                           .WithCodeFix<TwoActionCodeFix>()
                           .GenerateAsync();

        var fix = results.CodeFixResults[typeof(TwoActionCodeFix)].ResolvedFixes.Single();
        var actions = fix.CodeActions.ToDictionary(z => z.CodeAction.Title);
        await actions.Count.Should().BeEqualTo(2);
        await actions["Insert A"].TextChanges["Input0.cs"].Single().NewText.Should().BeEqualTo("/* A */");
        await actions["Insert B"].TextChanges["Input0.cs"].Single().NewText.Should().BeEqualTo("/* B */");
        await ( await actions["Insert B"].Changes.NewProject.Documents.Single().GetTextAsync() ).ToString().Should().BeEqualTo("/* B */public class A { }");
    }

    [Test]
    public async Task Each_Document_Fix_Has_Its_Own_Changes()
    {
        // TestAnalyzer reports one location-less diagnostic, so the fix is offered once per document.
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .AddSource("A.cs", "public class A { }")
                           .AddSource("B.cs", "public class B { }")
                           .WithAnalyzer<TestAnalyzer>()
                           .WithCodeFix<TestCodeFix>()
                           .GenerateAsync();

        var fixes = results.CodeFixResults[typeof(TestCodeFix)].ResolvedFixes;
        await fixes.Length.Should().BeEqualTo(2);
        var changes = fixes.Select(fix => (fix.Document.Name, fix.CodeActions.Single().TextChanges)).ToList();
        foreach ((var documentName, var textChanges) in changes)
        {
            await textChanges.Keys.Should().BeEquivalentTo([documentName,]);
            await textChanges[documentName][0].NewText.Should().BeEqualTo("NewText");
        }
    }

    [Test]
    public async Task Parse_Option_Setters_Keep_Features()
    {
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .WithFeature("my-feature", "true")
                           .WithLanguageVersion(LanguageVersion.CSharp12)
                           .WithDocumentationMode(DocumentationMode.Diagnose)
                           .AddPreprocessorSymbol("SOMETHING")
                           .WithSourceCodeKind(SourceCodeKind.Regular)
                           .GenerateAsync();

        await results.ParseOptions.Features.ContainsKey("my-feature").Should().BeTrue();
        await results.ParseOptions.LanguageVersion.Should().BeEqualTo(LanguageVersion.CSharp12);
        await results.ParseOptions.DocumentationMode.Should().BeEqualTo(DocumentationMode.Diagnose);
        await results.ParseOptions.PreprocessorSymbolNames.Contains("SOMETHING").Should().BeTrue();
    }

    [Test]
    public async Task WithFeature_Replaces_A_Repeated_Key()
    {
        var builder = Capture(
            () => GeneratorTestContextBuilder
                 .Create()
                 .WithFeature("my-feature", "1")
                 .WithFeature("my-feature", "2")
        );
        await builder.Exception.Should().BeNull();

        var results = await builder.Value!.GenerateAsync();
        await results.ParseOptions.Features["my-feature"].Should().BeEqualTo("2");
    }

    [Test]
    public async Task Refactoring_Finds_Markup_Added_Without_An_Extension()
    {
        var result = await GeneratorTestContextBuilder
                          .Create()
                          .AddMarkup("Code", "[*c*]")
                          .GenerateCodeRefactoring<TestRefactoring>();

        await result.ResolvedFixes.Length.Should().BeEqualTo(1);
        await result.ResolvedFixes[0].Document.Name.Should().BeEqualTo("Code.cs");
    }

    [Test]
    public async Task Code_Fix_Results_Serialize_When_Nested()
    {
        var result = await GeneratorTestContextBuilder
                          .Create()
                          .AddSources("")
                          .WithAnalyzer<TestAnalyzer>()
                          .GenerateCodeFix<TestCodeFix>();
        await Verify(new { Fix = result, });
    }

    [Test]
    public async Task Code_Refactoring_Results_Serialize_When_Nested()
    {
        var result = await GeneratorTestContextBuilder
                          .Create()
                          .AddMarkup("Code.cs", "[*c*]")
                          .GenerateCodeRefactoring<TestRefactoring>();
        await Verify(new { Refactoring = result, });
    }

    [Test]
    public async Task WithGenerator_Rejects_A_Type_That_Is_Not_A_Generator()
    {
#pragma warning disable CA2263 // the Type overload is the one under test: the generic overload's constraint cannot reject an analyzer
        var result = Capture(() => GeneratorTestContextBuilder.Create().WithGenerator(typeof(TestAnalyzer)));
#pragma warning restore CA2263
        await ( result.Exception is ArgumentException ).Should().BeTrue();
    }

    [Test]
    public async Task WithCodeRefactoring_Rejects_A_Type_That_Is_Not_A_Refactoring()
    {
        var result = Capture(() => GeneratorTestContextBuilder.Create().WithCodeRefactoring(typeof(TestCodeFix)));
        await ( result.Exception is ArgumentException ).Should().BeTrue();
    }

    [Test]
    public async Task WithCodeRefactoring_Of_T_Registers_The_Refactoring()
    {
        // Before the fix WithCodeRefactoring<T> went through WithCodeFix, which now rejects refactoring providers.
        var result = Capture(() => GeneratorTestContextBuilder.Create().WithCodeRefactoring<TestRefactoring>());
        await result.Exception.Should().BeNull();
    }

    [Test]
    public async Task AssertCompilationWasSuccessful_Lists_The_Failing_Diagnostics()
    {
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .AddSources("public class A { public Missing Value { get; set; } }")
                           .GenerateAsync();

        var exception = Capture(results.AssertCompilationWasSuccessful);
        await ( exception is InvalidOperationException ).Should().BeTrue();
        await exception!.Message.Should().Contain("CS0246");
        await exception.Message.Should().Contain("Missing");
    }

    [Test]
    public async Task EnsureDiagnosticSeverity_Lists_The_Failing_Generator_Diagnostics()
    {
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .WithGenerator<MyDiagnosticGenerator>()
                           .GenerateAsync();

        var exception = Capture(() => results.EnsureDiagnosticSeverity(DiagnosticSeverity.Error));
        await ( exception is InvalidOperationException ).Should().BeTrue();
        await exception!.Message.Should().Contain("TEST001");
        await exception.Message.Should().NotContain("TEST002");
    }

    [Test]
    public async Task AssertGenerationWasSuccessful_Lists_The_Failing_Generator_Diagnostics()
    {
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .WithGenerator<MyDiagnosticGenerator>()
                           .GenerateAsync();

        var exception = Capture(results.AssertGenerationWasSuccessful);
        await ( exception is InvalidOperationException ).Should().BeTrue();
        await exception!.Message.Should().Contain(typeof(MyDiagnosticGenerator).FullName!);
        await exception.Message.Should().Contain("TEST002");
    }

    internal static Exception? Capture(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    internal static (T? Value, Exception? Exception) Capture<T>(Func<T> action)
    {
        try
        {
            return (action(), null);
        }
        catch (Exception e)
        {
            return (default, e);
        }
    }
}
