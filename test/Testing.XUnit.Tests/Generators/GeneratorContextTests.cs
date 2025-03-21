﻿using System.Runtime.Loader;
using System.Text;
using FakeItEasy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Rocket.Surgery.Extensions.Testing.XUnit.Tests.Generators;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class GeneratorContextTests(ITestOutputHelper outputHelper) : LoggerTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

    [Fact]
    public async Task Should_Build_A_Context()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithLogger(Logger)
                     .WithProjectName("MyProject")
                     .WithLanguageVersion(LanguageVersion.Latest)
                     .WithDocumentationMode(DocumentationMode.Diagnose)
                     .WithAssemblyLoadContext(AssemblyLoadContext.Default)
                     .WithSourceCodeKind(SourceCodeKind.Regular)
                     .WithFeature("my-feature", "true")
                     .AddPreprocessorSymbol("SOMETHING", "SOMETHING_ELSE")
                     .WithCustomizer(Customizers.IncludeContextId)
                     .Build();
        _ = await Verify(context.GenerateAsync());
        context.AssemblyLoadContext.ShouldBe(AssemblyLoadContext.Default);
    }

    [Fact]
    public async Task Should_Add_Sources()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddSources("public class Test { }")
                     .AddSources(SourceText.From("public class Test2 { }", Encoding.UTF8))
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Not_Filter_Ouputs_Named_After_Input()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithGenerator<MyInputLikeSourceGenerator>()
                     .AddSources("public class Test { }")
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Named_Sources()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddSource("Test.cs", "public class Test { }")
                     .AddSource("Test2.cs", SourceText.From("public class Test2 { }", Encoding.UTF8))
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Options()
    {
        var customText = A.Fake<AdditionalText>();
        _ = A.CallTo(() => customText.Path).Returns("custom.txt");
        _ = A.CallTo(() => customText.GetText(A<CancellationToken>._)).Returns(SourceText.From("this is a text file", Encoding.UTF8));
        var customSyntax = A.Fake<SyntaxTree>();
        _ = A.CallTo(() => customSyntax.FilePath).Returns("custom.cs");
        _ = A.CallTo(() => customSyntax.GetText(A<CancellationToken>._)).Returns(SourceText.From("this is a text file", Encoding.UTF8));


        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddGlobalOption("my_option", "my_value")
                     .AddOption("my_file.cs", "my_option", "my_local_value")
                     .AddOption(customText, "custom_option", "custom_value")
                     .AddOption(customSyntax, "custom_option", "custom_value")
                     .AddSources(SourceText.From("public class Test2 { }", Encoding.UTF8))
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Additional_Texts()
    {
        var customText = A.Fake<AdditionalText>();
        _ = A.CallTo(() => customText.Path).Returns("custom.txt");
        _ = A.CallTo(() => customText.GetText(A<CancellationToken>._)).Returns(SourceText.From("this is a text file", Encoding.UTF8));
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddAdditionalText("some.csv", "a,b,c")
                     .AddAdditionalText("some-other.csv", SourceText.From("d,e,f", Encoding.UTF8))
                     .AddAdditionalTexts(customText)
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_References()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddReferences(GetType().Assembly)
                     .AddReferences(typeof(GeneratorTestResult))
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Analyzer()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithAnalyzer<TestAnalyzer>()
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_CodeFix()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddSources("")
                     .WithAnalyzer<TestAnalyzer>()
                     .WithCodeFix<TestCodeFix>()
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_CodeRefactoring()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddMarkup("Code.cs", "[*c*]")
                     .WithCodeRefactoring<TestRefactoring>()
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Generate_Analyzer()
    {
        var context = GeneratorTestContextBuilder
           .Create();
        _ = await Verify(context.GenerateAnalyzer<TestAnalyzer>());
    }

    [Fact]
    public async Task Should_Generate_CodeFix()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddSources("")
                     .WithAnalyzer<TestAnalyzer>()
                     .Build();
        _ = await Verify(context.GenerateCodeFix<TestCodeFix>());
    }

    [Fact]
    public async Task Should_Generate_CodeRefactoring()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddMarkup("Code.cs", "[*c*]")
                     .Build();
        _ = await Verify(context.GenerateCodeRefactoring<TestRefactoring>());
    }

    [Fact]
    public async Task Should_Add_Compilation_References()
    {
        var assemblyA = await GeneratorTestContextBuilder
                             .Create()
                             .WithProjectName("SampleDependencyOne")
                             .AddSources(
                                  @"namespace Sample.DependencyOne;

public class Class1
{
}
"
                              )
                             .Build()
                             .GenerateAsync();

        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddCompilationReferences(assemblyA)
                     .AddSources("public class A { public Sample.DependencyOne.Class1 Class1 { get; set; } }")
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Have_Diagnostics_For_Invalid_Code()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddSources("public class A { public Class1 Class1 { get; set; } }")
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Run_Source_Generator()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithLogger(Logger)
                     .WithGenerator<MySourceGenerator>()
                     .AddSources("public class A { public GeneratorTest Class1 { get; set; } }")
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Ignore_Output_File_If_Set()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithGenerator<MySourceGenerator>()
                     .IgnoreOutputFile("test.g.cs")
                     .AddSources("public class A { public GeneratorTest Class1 { get; set; } }")
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Run_Incremental_Source_Generator()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithLogger(Logger)
                     .WithGenerator<MyIncrementalGenerator>()
                     .AddSources("public class A { public GeneratorTest Class1 { get; set; } }")
                     .Build();
        _ = await Verify(context.GenerateAsync());
    }

    [Theory]
    [InlineData(DiagnosticSeverity.Error)]
    [InlineData(DiagnosticSeverity.Warning)]
    [InlineData(DiagnosticSeverity.Info)]
    [InlineData(DiagnosticSeverity.Hidden)]
    public async Task Should_Filter_Diagnostics(DiagnosticSeverity diagnosticSeverity)
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithLogger(Logger)
                     .WithDiagnosticSeverity(diagnosticSeverity)
                     .WithGenerator<MyDiagnosticGenerator>()
                     .Build();
        _ = await Verify(context.GenerateAsync()).UseParameters(diagnosticSeverity);
    }

    [Theory]
    [MemberData(nameof(GeneratorTestResultsCustomizerData))]
    public async Task Should_Support_Customizers(string name, GeneratorTestResultsCustomizer customizer)
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithLogger(Logger)
                     .WithCustomizer(customizer)
                     .AddOption("test.g.cs", "a", "value")
                     .AddGlobalOption("b", "key")
                     .IgnoreOutputFile("test.g.cs")
                     .AddSource("file.cs", "")
                     .WithGenerator<MySourceGenerator>()
                     .Build();
        _ = await Verify(context.GenerateAsync()).HashParameters().UseParameters(name);
    }

    public static IEnumerable<object[]> GeneratorTestResultsCustomizerData()
    {
        yield return new object[] { "IncludeInputs", Customizers.Reset + Customizers.IncludeInputs };
        yield return new object[] { "IncludeReferences", Customizers.Reset + Customizers.IncludeReferences };
        yield return new object[] { "IncludeFileOptions", Customizers.Reset + Customizers.IncludeFileOptions };
        yield return new object[] { "IncludeParseOptions", Customizers.Reset + Customizers.IncludeParseOptions };
        yield return new object[] { "IncludeGlobalOptions", Customizers.Reset + Customizers.IncludeGlobalOptions };
        yield return new object[] { "Default + IncludeInputs", Customizers.Reset + Customizers.Default + Customizers.IncludeInputs };
        yield return new object[] { "Default + ExcludeReferences", Customizers.Reset + Customizers.Default + Customizers.ExcludeReferences };
        yield return new object[] { "Default + ExcludeFileOptions", Customizers.Reset + Customizers.Default + Customizers.ExcludeFileOptions };
        yield return new object[] { "Default + ExcludeGlobalOptions", Customizers.Reset + Customizers.Default + Customizers.ExcludeGlobalOptions };
        yield return new object[] { "Default + ExcludeParseOptions", Customizers.Reset + Customizers.Default + Customizers.ExcludeParseOptions };
    }
}
