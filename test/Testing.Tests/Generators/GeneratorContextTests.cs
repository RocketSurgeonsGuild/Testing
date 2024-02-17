using System.Runtime.Loader;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests.Generators;

public class GeneratorContextTests : LoggerTest
{
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
                     .Build();
        await Verify(context.GenerateAsync());
        context.AssemblyLoadContext.Should().Be(AssemblyLoadContext.Default);
    }

    [Fact]
    public async Task Should_Add_Sources()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddSources("public class Test { }")
                     .AddSources(SourceText.From("public class Test2 { }", Encoding.UTF8))
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Named_Sources()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddSource("Test.cs", "public class Test { }")
                     .AddSource("Test2.cs", SourceText.From("public class Test2 { }", Encoding.UTF8))
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Options()
    {
        var customText = A.Fake<AdditionalText>();
        A.CallTo(() => customText.Path).Returns("custom.txt");
        A.CallTo(() => customText.GetText(A<CancellationToken>._)).Returns(SourceText.From("this is a text file", Encoding.UTF8));
        var customSyntax = A.Fake<SyntaxTree>();
        A.CallTo(() => customSyntax.FilePath).Returns("custom.cs");
        A.CallTo(() => customSyntax.GetText(A<CancellationToken>._)).Returns(SourceText.From("this is a text file", Encoding.UTF8));


        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddGlobalOption("my_option", "my_value")
                     .AddOption("my_file.cs", "my_option", "my_local_value")
                     .AddOption(customText, "custom_option", "custom_value")
                     .AddOption(customSyntax, "custom_option", "custom_value")
                     .AddSources(SourceText.From("public class Test2 { }", Encoding.UTF8))
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Additional_Texts()
    {
        var customText = A.Fake<AdditionalText>();
        A.CallTo(() => customText.Path).Returns("custom.txt");
        A.CallTo(() => customText.GetText(A<CancellationToken>._)).Returns(SourceText.From("this is a text file", Encoding.UTF8));
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddAdditionalText("some.csv", "a,b,c")
                     .AddAdditionalText("some-other.csv", SourceText.From("d,e,f", Encoding.UTF8))
                     .AddAdditionalTexts(customText)
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_References()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddReferences(GetType().Assembly)
                     .AddReferences(typeof(GeneratorTestResult))
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Compilation_References()
    {
        var assemblyA = GeneratorTestContextBuilder
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
                       .Compile();

        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddCompilationReferences(assemblyA)
                     .AddSources(@"public class A { public Sample.DependencyOne.Class1 Class1 { get; set; } }")
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Add_Have_Diagnostics_For_Invalid_Code()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .AddSources(@"public class A { public Class1 Class1 { get; set; } }")
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Run_Source_Generator()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithGenerator<MySourceGenerator>()
                     .AddSources(@"public class A { public GeneratorTest Class1 { get; set; } }")
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Ignore_Output_File_If_Set()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithGenerator<MySourceGenerator>()
                     .IgnoreOutputFile("test.g.cs")
                     .AddSources(@"public class A { public GeneratorTest Class1 { get; set; } }")
                     .Build();
        await Verify(context.GenerateAsync());
    }

    [Fact]
    public async Task Should_Run_Incremental_Source_Generator()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithGenerator<MyIncrementalGenerator>()
                     .AddSources(@"public class A { public GeneratorTest Class1 { get; set; } }")
                     .Build();
        await Verify(context.GenerateAsync());
    }

    public GeneratorContextTests(ITestOutputHelper outputHelper) : base(outputHelper, LogLevel.Trace) { }
}

public class MyIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(initializationContext => initializationContext.AddSource("test.g.cs", "public class GeneratorTest { }"));
    }
}

public class MySourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(z => z.AddSource("test.g.cs", "public class GeneratorTest { }"));
    }

    public void Execute(GeneratorExecutionContext context) { }
}