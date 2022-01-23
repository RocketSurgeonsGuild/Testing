//using FluentAssertions;
//using JetBrains.Annotations;
//using Microsoft.Extensions.Logging;
//using Analyzers.Tests.Helpers;
//using Rocket.Surgery.Extensions.Testing.Analyzers;
//using Xunit;
//using Xunit.Abstractions;
//
//namespace Analyzers.Tests
//{
//    public class InheritFromGeneratorTests : GeneratorTest
//    {
//        public InheritFromGeneratorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper, LogLevel.Trace)
//        {
//            WithGenerator<AbcGenerator>();
//        }
//
//        [Fact]
//        public async Task Should_Add_Sources_For_XUnit()
//        {
//            AddReferences(typeof(FactAttribute), typeof(Xunit.Abstractions.ITest));
//
//            var result = await GenerateAsync();
//            result.TryGetResult<AbcGenerator>(out var output).Should().BeTrue();
//            result.EnsureDiagnosticSeverity();
//            output.SyntaxTrees.Should().HaveCount(2);
//            output.SyntaxTrees.Should().Contain(z => z.FilePath.EndsWith("Marker_Rocket.Surgery.Extensions.Testing.XUnit.cs"))
//                  .And.Contain(z => z.FilePath.EndsWith("Rocket.Surgery.Extensions.Testing.XUnit_XUnitExtensions.cs"));
//        }
//
//        [Fact]
//        public async Task Should_Add_Sources_For_XUnit_Once()
//        {
//            AddReferences(typeof(FactAttribute), typeof(Xunit.Abstractions.ITest));
//            AddCompilationReference(new GeneratorTester("TestA", AssemblyLoadContext, TestOutputHelper).AddReferences(typeof(FactAttribute), typeof(Xunit.Abstractions.ITest)).WithGenerator<AbcGenerator>().Compile());
//
//            var result = await GenerateAsync();
//            result.TryGetResult<AbcGenerator>(out var output).Should().BeTrue();
//            result.EnsureDiagnosticSeverity();
//            output.SyntaxTrees.Should().HaveCount(0);
//        }
//
//    }
//}


