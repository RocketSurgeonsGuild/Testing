using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

public partial class AutoFixtureGenerator
{
    private static NamespaceDeclarationSyntax BuildNamespace(ISymbol namedTypeSymbol)
    {
        var displayString = namedTypeSymbol.ContainingNamespace.ToDisplayString();
        return NamespaceDeclaration(
                   ParseName(displayString)
               )
              .WithNamespaceKeyword(
                   Token(
                       TriviaList(
                           LineFeed
                       ),
                       SyntaxKind.NamespaceKeyword,
                       TriviaList(
                           Space
                       )
                   )
               )
              .WithOpenBraceToken(
                   Token(
                       TriviaList(),
                       SyntaxKind.OpenBraceToken,
                       TriviaList(
                           LineFeed
                       )
                   )
               );
    }

    private static ClassDeclarationSyntax BuildClassDeclaration(string fixtureName) =>
        ClassDeclaration(
                Identifier(
                    TriviaList(),
                    fixtureName,
                    TriviaList(
                        Space
                    )
                )
            )
           .WithModifiers(
                TokenList(
                    Token(
                        TriviaList(
                            LineFeed
                        ),
                        SyntaxKind.InternalKeyword,
                        TriviaList(
                            Space
                        )
                    ),
                    Token(
                        TriviaList(),
                        SyntaxKind.SealedKeyword,
                        TriviaList(
                            Space
                        )
                    ),
                    Token(
                        TriviaList(),
                        SyntaxKind.PartialKeyword,
                        TriviaList(
                            Space
                        )
                    )
                )
            )
           .WithKeyword(
                Token(
                    TriviaList(),
                    SyntaxKind.ClassKeyword,
                    TriviaList(
                        Space
                    )
                )
            )
           .WithBaseList(
                BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                GenericName(
                                        Identifier(
                                            TriviaList(),
                                            nameof(AutoFixtureBase),
                                            TriviaList(
                                                LineFeed
                                            )
                                        )
                                    )
                                   .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(
                                                IdentifierName(fixtureName)
                                            )
                                        )
                                    )
                            )
                        )
                    )
                   .WithColonToken(
                        Token(
                            TriviaList(),
                            SyntaxKind.ColonToken,
                            TriviaList(
                                Space
                            )
                        )
                    )
            )
           .WithOpenBraceToken(
                Token(
                    TriviaList(Tab),
                    SyntaxKind.OpenBraceToken,
                    TriviaList(
                        LineFeed
                    )
                )
            )
           .WithCloseBraceToken(Token(TriviaList(Tab), SyntaxKind.CloseBraceToken, TriviaList()))
           .WithTrailingTrivia(LineFeed);

    private static MemberDeclarationSyntax BuildFields(
        IParameterSymbol parameterSymbol,
        Compilation compilation
    )
    {
        var isAbstract = parameterSymbol.Type.IsAbstract;
        var isInterface = parameterSymbol.Type.TypeKind == TypeKind.Interface;

        var (fieldType, initializer) = GetFieldTypeAndInitializer(parameterSymbol, compilation);
        var symbolName = $"_{parameterSymbol.Name}";

        if (initializer is not null)
        {
            return FieldDeclaration(
                   VariableDeclaration(
                           IdentifierName(
                               Identifier(
                                   TriviaList(),
                                   fieldType,
                                   TriviaList(
                                       Space
                                   )
                               )
                           )
                       )
                      .WithVariables(
                           SingletonSeparatedList(
                               VariableDeclarator(
                                       Identifier(
                                           TriviaList(),
                                           symbolName,
                                           TriviaList(
                                               Space
                                           )
                                       )
                                   )
                                  .WithInitializer(
                                       EqualsValueClause(
                                               initializer
                                           )
                                          .WithEqualsToken(
                                               Token(
                                                   TriviaList(),
                                                   SyntaxKind.EqualsToken,
                                                   TriviaList(
                                                       Space
                                                   )
                                               )
                                           )
                                   )
                           )
                       )
               )
              .WithModifiers(
                   TokenList(
                       Token(
                           TriviaList(),
                           SyntaxKind.PrivateKeyword,
                           TriviaList(
                               Space
                           )
                       )
                   )
               )
              .WithTrailingTrivia(LineFeed);
        }

        var isKnownCollection = parameterSymbol.Type.Name is "IEnumerable" or "IList" or "ICollection" or "IReadOnlyList" or "IReadOnlyCollection";

        if (isKnownCollection && parameterSymbol.Type is INamedTypeSymbol { IsGenericType: true })
        {
             return FieldDeclaration(
                   VariableDeclaration(
                           IdentifierName(
                               Identifier(
                                   TriviaList(),
                                   fieldType,
                                   TriviaList(
                                       Space
                                   )
                               )
                           )
                       )
                      .WithVariables(
                           SingletonSeparatedList(
                               VariableDeclarator(
                                       Identifier(
                                           TriviaList(),
                                           symbolName,
                                           TriviaList(
                                               Space
                                           )
                                       )
                                   )
                                  .WithInitializer(
                                       EqualsValueClause(
                                               CollectionExpression(SeparatedList<CollectionElementSyntax>())
                                           )
                                          .WithEqualsToken(
                                               Token(
                                                   TriviaList(),
                                                   SyntaxKind.EqualsToken,
                                                   TriviaList(
                                                       Space
                                                   )
                                               )
                                           )
                                   )
                           )
                       )
               )
              .WithModifiers(
                   TokenList(
                       Token(
                           TriviaList(),
                           SyntaxKind.PrivateKeyword,
                           TriviaList(
                               Space
                           )
                       )
                   )
               )
              .WithTrailingTrivia(LineFeed);
        }

        return !isAbstract && !isInterface
            ? FieldDeclaration(
                    VariableDeclaration(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    fieldType,
                                    TriviaList(
                                        Space
                                    )
                                )
                            )
                        )
                       .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                        Identifier(symbolName)
                                    )
                                   .WithInitializer(
                                        EqualsValueClause(
                                            LiteralExpression(
                                                SyntaxKind.DefaultLiteralExpression,
                                                Token(SyntaxKind.DefaultKeyword)
                                            )
                                        )
                                    )
                            )
                        )
                )
               .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PrivateKeyword)
                    )
                )
            : FieldDeclaration(
                   VariableDeclaration(
                           IdentifierName(
                               Identifier(
                                   TriviaList(),
                                   fieldType,
                                   TriviaList(
                                       Space
                                   )
                               )
                           )
                       )
                      .WithVariables(
                           SingletonSeparatedList(
                               VariableDeclarator(
                                       Identifier(
                                           TriviaList(),
                                           symbolName,
                                           TriviaList(
                                               Space
                                           )
                                       )
                                   )
                                  .WithInitializer(
                                       EqualsValueClause(
                                               GetFieldInvocation(compilation, parameterSymbol)
                                           )
                                          .WithEqualsToken(
                                               Token(
                                                   TriviaList(),
                                                   SyntaxKind.EqualsToken,
                                                   TriviaList(
                                                       Space
                                                   )
                                               )
                                           )
                                   )
                           )
                       )
               )
              .WithModifiers(
                   TokenList(
                       Token(
                           TriviaList(),
                           SyntaxKind.PrivateKeyword,
                           TriviaList(
                               Space
                           )
                       )
                   )
               )
              .WithTrailingTrivia(LineFeed);
    }

    private static (string fieldType, ExpressionSyntax? initializer) GetFieldTypeAndInitializer(IParameterSymbol parameterSymbol, Compilation compilation)
    {
        var fieldType = parameterSymbol.Type.GetGenericDisplayName();
        ExpressionSyntax? initializer = null;

        if (parameterSymbol.Type is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
        {
            var originalDefinition = namedTypeSymbol.OriginalDefinition;
            var iEnumerable = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
            var iList = compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1");
            var iCollection = compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");
            var iReadOnlyList = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1");
            var iReadOnlyCollection = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");

            var isCollectionInterface = SymbolEqualityComparer.Default.Equals(originalDefinition, iEnumerable)
             || SymbolEqualityComparer.Default.Equals(originalDefinition, iList)
             || SymbolEqualityComparer.Default.Equals(originalDefinition, iCollection)
             || SymbolEqualityComparer.Default.Equals(originalDefinition, iReadOnlyList)
             || SymbolEqualityComparer.Default.Equals(originalDefinition, iReadOnlyCollection);

            if (!isCollectionInterface)
            {
                var fullMetadataName = originalDefinition.ToDisplayString();
                isCollectionInterface = fullMetadataName is "System.Collections.Generic.IEnumerable<T>"
                    or "System.Collections.Generic.IList<T>"
                    or "System.Collections.Generic.ICollection<T>"
                    or "System.Collections.Generic.IReadOnlyList<T>"
                    or "System.Collections.Generic.IReadOnlyCollection<T>";
            }

            if (isCollectionInterface)
            {
                initializer = CollectionExpression(SeparatedList<CollectionElementSyntax>());
            }
            else
            {
                var displayString = namedTypeSymbol.ToDisplayString();
                if (displayString.StartsWith("System.Collections.Generic.IEnumerable<") ||
                    displayString.StartsWith("System.Collections.Generic.IList<") ||
                    displayString.StartsWith("System.Collections.Generic.ICollection<") ||
                    displayString.StartsWith("System.Collections.Generic.IReadOnlyList<") ||
                    displayString.StartsWith("System.Collections.Generic.IReadOnlyCollection<"))
                {
                    initializer = CollectionExpression(SeparatedList<CollectionElementSyntax>());
                }
            }
        }

        return (fieldType, initializer);
    }

    private static MemberDeclarationSyntax BuildBuildMethod(
        string className,
        IEnumerable<IParameterSymbol> parameterSymbols
    )
    {
        List<SyntaxNodeOrToken> list = [];
        foreach (var parameterSymbol in parameterSymbols)
        {
            list.Add(Argument(IdentifierName($"_{parameterSymbol.Name}")));
            list.Add(Token(SyntaxKind.CommaToken));
        }

        if (list.Count > 0)
        {
            list.RemoveAt(list.Count - 1);
        }
        return GlobalStatement(
            LocalFunctionStatement(
                    IdentifierName(className),
                    Identifier("Build")
                )
               .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PrivateKeyword)
                    )
                )
               .WithExpressionBody(
                    ArrowExpressionClause(
                        ObjectCreationExpression(
                                IdentifierName(className)
                            )
                           .WithArgumentList(
                                ArgumentList(
                                    SeparatedList<ArgumentSyntax>(list)
                                )
                            )
                    )
                )
               .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken)
                )
        );
    }

    private static MemberDeclarationSyntax WithPropertyMethod(IParameterSymbol constructorParameter, string fixtureName, Compilation compilation)
    {
        var (fieldType, _) = GetFieldTypeAndInitializer(constructorParameter, compilation);
        return GlobalStatement(
            LocalFunctionStatement(
                    IdentifierName(
                        Identifier(
                            TriviaList(),
                            fixtureName,
                            TriviaList(
                                Space
                            )
                        )
                    ),
                    withTypeOrParameterName(constructorParameter)
                )
               .WithModifiers(
                    TokenList(
                        Token(
                            TriviaList(),
                            SyntaxKind.PublicKeyword,
                            TriviaList(
                                Space
                            )
                        )
                    )
                )
               .WithParameterList(
                    ParameterList(
                            SingletonSeparatedList(
                                Parameter(
                                        Identifier(constructorParameter.Name)
                                    )
                                   .WithType(
                                        IdentifierName(
                                            Identifier(
                                                TriviaList(),
                                                fieldType,
                                                TriviaList(
                                                    Space
                                                )
                                            )
                                        )
                                    )
                            )
                        )
                       .WithCloseParenToken(
                            Token(
                                TriviaList(),
                                SyntaxKind.CloseParenToken,
                                TriviaList(
                                    Space
                                )
                            )
                        )
                )
               .WithExpressionBody(
                    ArrowExpressionClause(
                            InvocationExpression(
                                    IdentifierName("With")
                                )
                               .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]
                                            {
                                                Argument(
                                                        IdentifierName($"_{constructorParameter.Name}")
                                                    )
                                                   .WithRefOrOutKeyword(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.RefKeyword,
                                                            TriviaList(
                                                                Space
                                                            )
                                                        )
                                                    ),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.CommaToken,
                                                    TriviaList(
                                                        Space
                                                    )
                                                ),
                                                Argument(
                                                    IdentifierName(constructorParameter.Name)
                                                ),
                                            }
                                        )
                                    )
                                )
                        )
                       .WithArrowToken(
                            Token(
                                TriviaList(),
                                SyntaxKind.EqualsGreaterThanToken,
                                TriviaList(
                                    Space
                                )
                            )
                        )
                )
               .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken)
                )
               .WithTrailingTrivia(LineFeed)
        );

        SyntaxToken withTypeOrParameterName(IParameterSymbol parameterSymbol)
        {
            var primitiveName = $"{ char.ToUpper(parameterSymbol.Name[0]).ToString()}{parameterSymbol.Name[1..]}";
            var splitLastCamel = useParameterName(parameterSymbol) ? primitiveName : SplitLastCamel(parameterSymbol);
            return Identifier($"With{splitLastCamel}");
        }

        bool useParameterName(IParameterSymbol parameterSymbol) => parameterSymbol.Type.TypeKind != TypeKind.Interface
            || parameterSymbol.Type.IsValueType
            || !parameterSymbol.Type.IsAbstract;
    }

    private static MemberDeclarationSyntax BuildOperator(string className, string fixtureName) =>
        ConversionOperatorDeclaration(
                Token(
                    TriviaList(),
                    SyntaxKind.ImplicitKeyword,
                    TriviaList(
                        Space
                    )
                ),
                IdentifierName(className)
            )
           .WithModifiers(
                TokenList(
                    Token(
                        TriviaList(),
                        SyntaxKind.PublicKeyword,
                        TriviaList(
                            Space
                        )
                    ),
                    Token(
                        TriviaList(),
                        SyntaxKind.StaticKeyword,
                        TriviaList(
                            Space
                        )
                    )
                )
            )
           .WithOperatorKeyword(
                Token(
                    TriviaList(),
                    SyntaxKind.OperatorKeyword,
                    TriviaList(
                        Space
                    )
                )
            )
           .WithParameterList(
                ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier(Fixture.ToLowerInvariant())
                                )
                               .WithType(
                                    IdentifierName(
                                        Identifier(
                                            TriviaList(),
                                            fixtureName,
                                            TriviaList(
                                                Space
                                            )
                                        )
                                    )
                                )
                        )
                    )
                   .WithCloseParenToken(
                        Token(
                            TriviaList(),
                            SyntaxKind.CloseParenToken,
                            TriviaList(
                                Space
                            )
                        )
                    )
            )
           .WithExpressionBody(
                ArrowExpressionClause(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(Fixture.ToLowerInvariant()),
                                IdentifierName("Build")
                            )
                        )
                    )
                   .WithArrowToken(
                        Token(
                            TriviaList(),
                            SyntaxKind.EqualsGreaterThanToken,
                            TriviaList(
                                Space
                            )
                        )
                    )
            )
           .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken)
            )
           .WithTrailingTrivia(LineFeed);

    private static string SplitLastCamel(IParameterSymbol typeSymbol) =>
        Regex
           .Replace(typeSymbol.Type.Name, "([A-Z])", " $1", RegexOptions.Compiled)
           .Trim()
           .Split(' ')
           .Last();

    private static InvocationExpressionSyntax GetFieldInvocation(Compilation compilation, IParameterSymbol symbol)
    {
        var fakeItEasy = compilation.GetTypeByMetadataName("FakeItEasy.Fake");

        return fakeItEasy is { }
            ? InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("A"),
                    GenericName(
                            Identifier("Fake")
                        )
                       .WithTypeArgumentList(
                            typeArgumentListSyntax(symbol)
                        )
                )
            )
            : InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(
                    "Substitute"
                ),
                GenericName(
                        Identifier(
                            "For"
                        )
                    )
                   .WithTypeArgumentList(
                        typeArgumentListSyntax(
                            symbol
                        )
                    )
            )
        );
        static TypeArgumentListSyntax typeArgumentListSyntax(IParameterSymbol parameterSymbol) => TypeArgumentList(
            SingletonSeparatedList<TypeSyntax>(
                ParseName(parameterSymbol.Type.GetGenericDisplayName())
            )
            );
    }

    private const string Fixture = nameof(Fixture);

    private static INamedTypeSymbol? GetClassForFixture(GeneratorAttributeSyntaxContext syntaxContext)
    {
        var targetSymbol = syntaxContext.TargetSymbol as INamedTypeSymbol;

        return syntaxContext.Attributes[0].ConstructorArguments.Length == 0
            ? targetSymbol
            : syntaxContext.Attributes[0].ConstructorArguments[0].Value is INamedTypeSymbol namedTypeSymbol ? namedTypeSymbol : null;
    }

    private static bool ReportAutoFixture0001(INamedTypeSymbol classForFixture, SourceProductionContext productionContext) => classForFixture.Constructors.All(methodSymbol => methodSymbol.Parameters.IsDefaultOrEmpty);

    private static bool ReportAutoFixture0002(INamedTypeSymbol namedTypeSymbol, SourceProductionContext productionContext)
    {
        const bool reported = false;
        //        foreach (var location in namedTypeSymbol
        //                                .Constructors
        //                                .SelectMany(methodSymbol => methodSymbol.Parameters)
        //                                .Distinct(ParameterReductionComparer.Default)
        //                                .Select(parameterSymbol => new { parameterSymbol, isArrayType = parameterSymbol.Type is IArrayTypeSymbol })
        //                                .Select(
        //                                     tuple => new
        //                                     {
        //                                         tuple.isArrayType,
        //                                         tuple.parameterSymbol,
        //                                         hasParamsKeyWord = tuple.parameterSymbol.ToDisplayString().Contains("params"),
        //                                     }
        //                                 )
        //                                .Where(tuple => tuple.isArrayType && tuple.hasParamsKeyWord)
        //                                .SelectMany(tuple => tuple.parameterSymbol.Locations))
        //        {
        //            productionContext.ReportDiagnostic(Diagnostic.Create(Rsaf0002.Descriptor, location));
        //            if (!reported)
        //            {
        //                reported = true;
        //            }
        //        }

        return reported;
    }
}
