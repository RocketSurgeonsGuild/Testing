using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

[Generator]
public partial class AutoFixtureGenerator : IIncrementalGenerator //, ISourceGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider =
            context
               .SyntaxProvider
               .ForAttributeWithMetadataName(
                    "Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureAttribute",
                    (node, token) => node.IsKind(SyntaxKind.ClassDeclaration),
                    (syntaxContext, token) => syntaxContext
                )
               .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(syntaxProvider, generateFixtureBuilder);

        // do generator things
        context.RegisterPostInitializationOutput(
            initializationContext =>
            {
                initializationContext.AddSource("AutoFixtureAttribute.g.cs", Attribute.Source);
                initializationContext.AddSource($"{nameof(AutoFixtureBase)}.g.cs", AutoFixtureBase.Source);
            }
        );

        void generateFixtureBuilder(
            SourceProductionContext productionContext,
            (GeneratorAttributeSyntaxContext context, Compilation compilation) valueTuple
        )
        {
            ( var syntaxContext, var compilation ) = valueTuple;

            var substituteMetadata = compilation.GetTypeByMetadataName("NSubstitute.Substitute");
            var fakeItEasy = compilation.GetTypeByMetadataName("FakeItEasy.Fake");

            if (syntaxContext.Attributes[0].ConstructorArguments[0].Value is not INamedTypeSymbol namedTypeSymbol)
            {
                return;
            }

            var parameterSymbols =
                namedTypeSymbol
                   .Constructors
                   .SelectMany(methodSymbol => methodSymbol.Parameters)
                   .Distinct(ParameterReductionComparer.Default)
                   .ToList();

            foreach (var location in parameterSymbols
                                    .Select(parameterSymbol => new { parameterSymbol, isArrayType = parameterSymbol.Type is IArrayTypeSymbol, })
                                    .Select(
                                         tuple => new
                                         {
                                             tuple.parameterSymbol, tuple.isArrayType,
                                             hasParamsKeyWord = tuple.parameterSymbol.ToDisplayString().Contains("params"),
                                         }
                                     )
                                    .Where(tuple => tuple.isArrayType && tuple.hasParamsKeyWord)
                                    .SelectMany(tuple => tuple.parameterSymbol.Locations))
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(Diagnostics.AutoFixture0001, location));
                return;
            }

            var fullList =
                new[] { Operator(namedTypeSymbol), }
                   .Concat(parameterSymbols.Select(WithPropertyMethod))
                   .Concat(new[] { BuildBuildMethod(namedTypeSymbol, parameterSymbols), })
                   .Concat(
                        parameterSymbols.Select(symbol => BuildFields(symbol, GetFieldInvocation(compilation, symbol)))
                    );

            var classDeclaration = BuildClassDeclaration(namedTypeSymbol)
               .WithMembers(new(fullList));

            var namespaceDeclaration = BuildNamespace(syntaxContext.TargetSymbol)
               .WithMembers(new(classDeclaration));

            var usings =
                parameterSymbols
                   .Select(symbol => symbol.Type.ContainingNamespace?.ToDisplayString() ?? string.Empty)
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Distinct()
                   .OrderBy(x => x)
                   .Select(x => UsingDirective(ParseName(x)))
                   .ToArray();

            var mockLibrary = UsingDirective(
                ParseName(
                    ( fakeItEasy is { }
                        ? fakeItEasy.ContainingNamespace
                        : substituteMetadata?.ContainingNamespace )
                  ?.ToDisplayString()
                 ?? string.Empty
                )
            );
            var unit =
                CompilationUnit()
                   .AddUsings(UsingDirective(ParseName("System.Collections.ObjectModel")))
                   .AddUsings(usings)
                   .AddUsings(mockLibrary)
                   .AddUsings(UsingDirective(ParseName("Rocket.Surgery.Extensions.Testing.AutoFixtures")))
                   .AddMembers(namespaceDeclaration)
                   .NormalizeWhitespace();

            productionContext.AddSource($"{namedTypeSymbol.Name}.AutoFixture.g.cs", unit.ToFullString());
        }
    }

    internal class ParameterReductionComparer : IEqualityComparer<IParameterSymbol>
    {
        public static IEqualityComparer<IParameterSymbol> Default { get; } = new ParameterReductionComparer();

        public bool Equals(IParameterSymbol x, IParameterSymbol y)
        {
            return ( x.Type.Equals(y.Type) && x.Name.Equals(y.Name) ) || SymbolEqualityComparer.Default.Equals(x, y);
        }

        public int GetHashCode(IParameterSymbol obj)
        {
            return SymbolEqualityComparer.Default.GetHashCode(obj.Type) + obj.Type.GetHashCode() + obj.Name.GetHashCode();
        }
    }
}