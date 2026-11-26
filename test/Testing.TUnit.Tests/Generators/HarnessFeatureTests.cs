using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests.Generators;

/// <summary>
///     Tests for compilation options, incremental step tracking and reruns, and emit results.
/// </summary>
public class HarnessFeatureTests() : LoggerTest<TestRecord>(TestRecord.Create())
{
    private const string UnusedVariable = "public class A { public void M() { int x; } }";

    [Test]
    public async Task Nullable_Context_Is_Applied()
    {
        var builder = GeneratorTestContextBuilder.Create().AddSources("public class A { public string? S; }");

        var disabled = await builder.GenerateAsync();
        var enabled = await builder.WithNullableContext(NullableContextOptions.Enable).GenerateAsync();

        await disabled.FinalDiagnostics.Any(z => z.Id == "CS8632").Should().BeTrue();
        await enabled.FinalDiagnostics.Any(z => z.Id == "CS8632").Should().BeFalse();
        await enabled.FinalCompilation.Options.NullableContextOptions.Should().BeEqualTo(NullableContextOptions.Enable);
        await enabled.ProjectInformation.SourceProject.CompilationOptions!.NullableContextOptions.Should().BeEqualTo(NullableContextOptions.Enable);
    }

    [Test]
    public async Task Unsafe_Code_Is_Allowed()
    {
        var builder = GeneratorTestContextBuilder.Create().AddSources("public unsafe class A { public int* P; }");

        var disallowed = await builder.GenerateAsync();
        var allowed = await builder.WithAllowUnsafe().GenerateAsync();

        await disallowed.FinalDiagnostics.Any(z => z.Id == "CS0227").Should().BeTrue();
        await allowed.FinalDiagnostics.Any(z => z.Severity == DiagnosticSeverity.Error).Should().BeFalse();
        await allowed.Assembly.Should().NotBeNull();
    }

    [Test]
    public async Task Optimization_Level_Is_Applied()
    {
        var results = await GeneratorTestContextBuilder.Create().WithOptimizationLevel(OptimizationLevel.Release).AddSources("public class A { }").GenerateAsync();
        await results.FinalCompilation.Options.OptimizationLevel.Should().BeEqualTo(OptimizationLevel.Release);
    }

    [Test]
    public async Task Compilation_Options_Can_Be_Configured_Directly()
    {
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .WithCompilationOptions(z => z.WithOverflowChecks(true).WithAllowUnsafe(true))
                           .AddSources("public class A { }")
                           .GenerateAsync();
        await results.FinalCompilation.Options.CheckOverflow.Should().BeTrue();
        await results.FinalCompilation.Options.AllowUnsafe.Should().BeTrue();
    }

    [Test]
    public async Task Diagnostic_Option_Changes_Severity()
    {
        var results = await GeneratorTestContextBuilder.Create().WithDiagnosticOption("CS0168", ReportDiagnostic.Error).AddSources(UnusedVariable).GenerateAsync();
        await results.FinalDiagnostics.Single(z => z.Id == "CS0168").Severity.Should().BeEqualTo(DiagnosticSeverity.Error);
    }

    [Test]
    public async Task Global_Severity_Option_Changes_Compiler_Severity()
    {
        var results = await GeneratorTestContextBuilder.Create().AddGlobalOption("dotnet_diagnostic.CS0168.severity", "error").AddSources(UnusedVariable).GenerateAsync();
        await results.FinalDiagnostics.Single(z => z.Id == "CS0168").Severity.Should().BeEqualTo(DiagnosticSeverity.Error);
    }

    [Test]
    public async Task Global_Severity_Option_Changes_Analyzer_Severity()
    {
        var builder = GeneratorTestContextBuilder.Create().AddSources("public class A { }").WithAnalyzer<TestAnalyzer>();

        var raised = await builder.AddGlobalOption("dotnet_diagnostic.TEST0001.severity", "error").GenerateAsync();
        var suppressed = await builder.AddGlobalOption("dotnet_diagnostic.TEST0001.severity", "none").GenerateAsync();

        await raised.AnalyzerResults[typeof(TestAnalyzer)].Diagnostics.Single().Severity.Should().BeEqualTo(DiagnosticSeverity.Error);
        await suppressed.FinalDiagnostics.Any(z => z.Id == "TEST0001").Should().BeFalse();
    }

    [Test]
    public async Task Global_Severity_Option_Changes_Generator_Severity()
    {
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .AddGlobalOption("dotnet_diagnostic.TEST002.severity", "error")
                           .WithGenerator<MyDiagnosticGenerator>()
                           .GenerateAsync();
        await results.Results[typeof(MyDiagnosticGenerator)].Diagnostics.Single(z => z.Id == "TEST002").Severity.Should().BeEqualTo(DiagnosticSeverity.Error);
    }

    [Test]
    public async Task File_Severity_Option_Applies_Only_To_That_File()
    {
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .AddSource("A.cs", UnusedVariable)
                           .AddSource("B.cs", UnusedVariable.Replace("class A", "class B", StringComparison.Ordinal))
                           .AddOption("A.cs", "dotnet_diagnostic.CS0168.severity", "none")
                           .GenerateAsync();

        var unused = results.FinalDiagnostics.Where(z => z.Id == "CS0168").ToList();
        await unused.Count.Should().BeEqualTo(1);
        await unused[0].Location.SourceTree!.FilePath.Should().BeEqualTo("B.cs");
    }

    [Test]
    public async Task Context_Id_Changes_Only_With_Non_Default_Compilation_Options()
    {
        var builder = GeneratorTestContextBuilder.Create().AddSources("public class A { }");
        using var plain = builder.Build();
        using var same = builder.WithCompilationOptions(z => z).Build();
        using var changed = builder.WithAllowUnsafe().Build();

        await same.Id.Should().BeEqualTo(plain.Id);
        await ( changed.Id != plain.Id ).Should().BeTrue();
    }

    [Test]
    public async Task Driver_Is_Kept_Without_Tracking_By_Default()
    {
        var results = await GeneratorTestContextBuilder.Create().WithGenerator<ClassNamesGenerator>().AddSources("public class A { }").GenerateAsync();
        var result = results.Results[typeof(ClassNamesGenerator)];

        await result.Driver.Should().NotBeNull();
        await result.RunResult.HasValue.Should().BeTrue();
        await result.RunResult!.Value.GeneratedSources.Single().HintName.Should().BeEqualTo("ClassNames.g.cs");
        await result.RunResult.Value.TrackedSteps.Count.Should().BeEqualTo(0);
    }

    [Test]
    public async Task Incremental_Tracking_Records_Steps()
    {
        var results = await GeneratorTestContextBuilder
                           .Create()
                           .WithIncrementalTracking()
                           .WithGenerator<ClassNamesGenerator>()
                           .AddSources("public class A { }")
                           .GenerateAsync();

        await results.Results[typeof(ClassNamesGenerator)].RunResult!.Value.TrackedSteps.ContainsKey(ClassNamesGenerator.StepName).Should().BeTrue();
    }

    [Test]
    public async Task Rerun_Without_Changes_Is_Cached()
    {
        var first = await GeneratorTestContextBuilder
                         .Create()
                         .WithIncrementalTracking()
                         .WithGenerator<ClassNamesGenerator>()
                         .AddSources("public class A { }", "public class B { }")
                         .GenerateAsync();

        var second = await first.RerunAsync();

        var reasons = Reasons(second, typeof(ClassNamesGenerator), ClassNamesGenerator.StepName);
        await reasons.Count.Should().BeEqualTo(2);
        await reasons.All(z => z is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged).Should().BeTrue();
    }

    [Test]
    public async Task Rerun_Sees_An_Edited_Compilation()
    {
        var first = await GeneratorTestContextBuilder
                         .Create()
                         .WithIncrementalTracking()
                         .WithGenerator<ClassNamesGenerator>()
                         .AddSources("public class A { }")
                         .GenerateAsync();

        var second = await first.RerunAsync(c => c.AddSyntaxTrees(CSharpSyntaxTree.ParseText("public class C { }", first.ParseOptions, "C.cs")));

        var reasons = Reasons(second, typeof(ClassNamesGenerator), ClassNamesGenerator.StepName);
        await reasons.Contains(IncrementalStepRunReason.New).Should().BeTrue();
        // The class that was already there is not recomputed into anything new.
        await reasons.Count(z => z is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged).Should().BeEqualTo(1);
        await Generated(second, typeof(ClassNamesGenerator)).Should().Contain("A,C");
        await Generated(first, typeof(ClassNamesGenerator)).Should().NotContain("C");
        await second.InputSyntaxTrees.Length.Should().BeEqualTo(2);
        await second.Assembly.Should().BeNull();
        await second.MetadataReference.Should().NotBeNull();
        await second.Image.IsEmpty.Should().BeFalse();
    }

    [Test]
    public async Task Rerun_Sees_A_Replaced_Reference()
    {
        var libraryOne = await GeneratorTestContextBuilder.Create().WithProjectName("LibraryOne").AddSources("public class One { }").GenerateAsync();
        var libraryTwo = await GeneratorTestContextBuilder.Create().WithProjectName("LibraryTwo").AddSources("public class Two { }").GenerateAsync();

        var first = await GeneratorTestContextBuilder
                         .Create()
                         .WithIncrementalTracking()
                         .WithGenerator<ReferenceNamesGenerator>()
                         .AddCompilationReferences(libraryOne)
                         .AddSources("public class A { }")
                         .GenerateAsync();

        var second = await first.RerunAsync(c => c.ReplaceReference(libraryOne.MetadataReference!, libraryTwo.MetadataReference!));

        await Generated(first, typeof(ReferenceNamesGenerator)).Should().Contain("LibraryOne");
        await Generated(second, typeof(ReferenceNamesGenerator)).Should().Contain("LibraryTwo");
        await Reasons(second, typeof(ReferenceNamesGenerator), ReferenceNamesGenerator.StepName).Single().Should().BeEqualTo(IncrementalStepRunReason.Modified);
    }

    [Test]
    public async Task Emit_Result_And_Image_Are_Exposed()
    {
        var results = await GeneratorTestContextBuilder.Create().AddSources("public class A { }").GenerateAsync();

        await results.EmitResult!.Success.Should().BeTrue();
        await results.Image.IsEmpty.Should().BeFalse();
        await results.Assembly.Should().NotBeNull();
    }

    [Test]
    public async Task Emit_Failure_Is_Surfaced()
    {
        var results = await GeneratorTestContextBuilder.Create().AddSources("public class A { public Missing Value { get; set; } }").GenerateAsync();

        await results.EmitResult!.Success.Should().BeFalse();
        await results.EmitDiagnostics.Any(z => z.Id == "CS0246").Should().BeTrue();
        await results.Image.IsEmpty.Should().BeTrue();
        await results.Assembly.Should().BeNull();
        await results.MetadataReference.Should().BeNull();
    }

    [Test]
    public async Task AddCompilationReferences_Explains_A_Failed_Emit()
    {
        var broken = await GeneratorTestContextBuilder
                          .Create()
                          .WithProjectName("BrokenLibrary")
                          .AddSources("public class A { public Missing Value { get; set; } }")
                          .GenerateAsync();

        var exception = HarnessBugTests.Capture(() => { _ = GeneratorTestContextBuilder.Create().AddCompilationReferences(broken); });

        await ( exception is ArgumentException ).Should().BeTrue();
        await exception!.Message.Should().Contain("BrokenLibrary");
        await exception.Message.Should().Contain("CS0246");
    }

    [Test]
    public async Task AssertCompilationWasSuccessful_Can_Allow_Warnings()
    {
        var results = await GeneratorTestContextBuilder.Create().AddSources(UnusedVariable).GenerateAsync();

        var strict = HarnessBugTests.Capture(results.AssertCompilationWasSuccessful);
        var lenient = HarnessBugTests.Capture(() => results.AssertCompilationWasSuccessful(DiagnosticSeverity.Error));

        await ( strict is InvalidOperationException ).Should().BeTrue();
        await strict!.Message.Should().Contain("CS0168");
        await lenient.Should().BeNull();
    }

    private static List<IncrementalStepRunReason> Reasons(GeneratorTestResults results, Type generator, string stepName) =>
        [.. results
           .Results[generator]
           .RunResult!.Value.TrackedSteps[stepName]
           .SelectMany(z => z.Outputs)
           .Select(z => z.Reason)];

    private static string Generated(GeneratorTestResults results, Type generator) =>
        results.Results[generator].RunResult!.Value.GeneratedSources.Single().SourceText.ToString();
}
