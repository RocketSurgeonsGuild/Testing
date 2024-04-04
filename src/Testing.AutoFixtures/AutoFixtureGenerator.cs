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
                initializationContext.AddSource("AutoFixtureAttribute.g.cs", Attribute.source);
                initializationContext.AddSource($"{nameof(AutoFixtureBase)}.g.cs", AutoFixtureBase.Source);
            }
        );

        void generateFixtureBuilder(
            SourceProductionContext productionContext,
            (GeneratorAttributeSyntaxContext context, Compilation compilation) valueTuple
        )
        {
            ( var syntaxContext, var compilation ) = valueTuple;

            INamedTypeSymbol? classForFixture = GetClassForFixture(syntaxContext);

            if (classForFixture is null)
            {
                return;
            }

            // Check for all occurrences of known issues
            if (ReportAutoFixture0001(classForFixture, productionContext))
            {
                return;
            }

            if (ReportAutoFixture0002(classForFixture, productionContext))
            {
                return;
            }

            // Loop through
            // Check for a list of diagnostics to report
            // report and exit
            // Generate Stuffs™
            CurrentGenerator(classForFixture, syntaxContext.TargetSymbol, productionContext, compilation);
        }
    }

    private static void CurrentGenerator(
        INamedTypeSymbol namedTypeSymbol,
        ISymbol targetSymbol,
        SourceProductionContext productionContext,
        Compilation compilation
    )
    {

        var parameterSymbols =
            namedTypeSymbol
               .Constructors
               .SelectMany(methodSymbol => methodSymbol.Parameters)
               .Distinct(ParameterReductionComparer.Default)
               .ToList();

        var fullList =
            new[] { Operator(namedTypeSymbol), }
               .Concat(parameterSymbols.Select(WithPropertyMethod))
               .Concat(new[] { BuildBuildMethod(productionContext, namedTypeSymbol, parameterSymbols), })
               .Concat(
                    parameterSymbols.Select(symbol => BuildFields(symbol, compilation))
                );

        var classDeclaration = BuildClassDeclaration(namedTypeSymbol)
           .WithMembers(new(fullList));

        var namespaceDeclaration = BuildNamespace(targetSymbol)
           .WithMembers(new(classDeclaration));

        var usingDirectives = new HashSet<string>(
            parameterSymbols
               .Select(symbol => symbol.Type.ContainingNamespace?.ToDisplayString() ?? string.Empty)
               .Where(x => !string.IsNullOrWhiteSpace(x))
               .Distinct()
        ) { "System.Collections.ObjectModel", "Rocket.Surgery.Extensions.Testing.AutoFixtures", };

        var fakeItEasy = compilation.GetTypeByMetadataName("FakeItEasy.Fake");
        if (fakeItEasy is { })
        {
            usingDirectives.Add(fakeItEasy.ContainingNamespace.ToDisplayString());
        }

        var substituteMetadata = compilation.GetTypeByMetadataName("NSubstitute.Substitute");
        if (substituteMetadata is { })
        {
            usingDirectives.Add(substituteMetadata.ContainingNamespace.ToDisplayString());
        }

        var usingDirectiveSyntax =
            usingDirectives
               .OrderBy(usingDirective => usingDirective, NamespaceComparer.Default)
               .Select(x => UsingDirective(ParseName(x)))
               .ToArray();

        var unit =
            CompilationUnit()
               .AddUsings(usingDirectiveSyntax)
               .AddMembers(namespaceDeclaration)
               .NormalizeWhitespace();

        productionContext.AddSource($"{namedTypeSymbol.Name}.AutoFixture.g.cs", unit.ToFullString());
    }

    internal class ParameterReductionComparer : IEqualityComparer<IParameterSymbol>
    {
        public static IEqualityComparer<IParameterSymbol> Default { get; } = new ParameterReductionComparer();

        public bool Equals(IParameterSymbol x, IParameterSymbol y) => ( x.Type.Equals(y.Type) && x.Name.Equals(y.Name) ) || SymbolEqualityComparer.Default.Equals(x, y);

        public int GetHashCode(IParameterSymbol obj) => SymbolEqualityComparer.Default.GetHashCode(obj.Type) + obj.Type.GetHashCode() + obj.Name.GetHashCode();
    }

    internal class NamespaceComparer : IComparer<string>
    {
        public static NamespaceComparer Default { get; } = new();

        public int Compare(string x, string y)
        {
            // Check if both namespaces start with "System"
            var xIsSystem = x.StartsWith("System", StringComparison.Ordinal);
            var yIsSystem = y.StartsWith("System", StringComparison.Ordinal);

            return xIsSystem switch
                   {
                       // If only one of them starts with "System", prioritize it
                       true when !yIsSystem => -1,
                       false when yIsSystem => 1,
                       // If both start with "System" or neither does, compare them alphabetically
                       true when yIsSystem   => string.Compare(x, y, StringComparison.Ordinal),
                       false when !yIsSystem => string.Compare(x, y, StringComparison.Ordinal),
                       _                     => xIsSystem ? -1 : 1,
                   };
        }
    }
}
