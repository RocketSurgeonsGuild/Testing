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
                    "Rocket.Surgery.Extensions.Testing.AutoFixture.AutoFixtureAttribute",
                    (node, token) => node.IsKind(SyntaxKind.ClassDeclaration),
                    (syntaxContext, token) => syntaxContext
                )
               .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(syntaxProvider, GenerateFixtureBuilder);

        // do generator things
        context.RegisterPostInitializationOutput(
            initializationContext =>
            {
                initializationContext.AddSource("AutoFixtureAttribute.g.cs", Attribute.Source);
                initializationContext.AddSource($"{nameof(BuilderInterface)}.g.cs", BuilderInterface.Source);
            }
        );

        void GenerateFixtureBuilder(
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
                   .Distinct(SymbolEqualityComparer.Default)
                   .OfType<IParameterSymbol>()
                   .ToList();

            foreach (var location in parameterSymbols
                                    .Select(parameterSymbol => new { parameterSymbol, isArrayType = parameterSymbol.Type is IArrayTypeSymbol })
                                    .Select(tuple => new { tuple.parameterSymbol, tuple.isArrayType, hasParamsKeyWord = tuple.parameterSymbol.ToDisplayString().Contains("params") })
                                    .Where(tuple => tuple.isArrayType && tuple.hasParamsKeyWord)
                                    .SelectMany(tuple => tuple.parameterSymbol.Locations))
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(Diagnostics.AutoFixture0001, location));
                return;
            }

            var fullList =
                new[] { Operator(namedTypeSymbol), }
                   .Concat(parameterSymbols.Select(symbol => WithPropertyMethod(symbol)))
                   .Concat(FixtureWithMethods.BuildFixtureMethods(namedTypeSymbol))
                   .Concat(new[] { BuildBuildMethod(namedTypeSymbol, parameterSymbols), })
                   .Concat(
                        parameterSymbols.Select(symbol => BuildFields(symbol, GetFieldInvocation(compilation, symbol)))
                    );

            var classDeclaration = BuildClassDeclaration(namedTypeSymbol)
               .WithMembers(new(fullList));

            // TODO: [rlittlesii: March 01, 2024] Configure use of same namespace, or define a namespace, or add suffix.
            var namespaceDeclaration = BuildNamespace(namedTypeSymbol)
               .WithMembers(new(classDeclaration));

            var usings =
                parameterSymbols
                   .Select(symbol => symbol.Type.ContainingNamespace?.ToDisplayString())
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Distinct()
                   .OrderBy(x => x)
                   .Select(x => UsingDirective(ParseName(x!)))
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
                   .AddUsings(mockLibrary)
                   .AddUsings(UsingDirective(ParseName("System.Collections.ObjectModel")))
                   .AddUsings(usings)
                   .AddMembers(namespaceDeclaration)
                   .NormalizeWhitespace();

            productionContext.AddSource($"{namedTypeSymbol.Name}.AutoFixture.g.cs", unit.ToFullString());
        }
    }
}
