using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;
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
        var isAbstract = parameterSymbol.IsAbstract;
        var isInterface = parameterSymbol.Type.TypeKind == TypeKind.Interface;

        var symbolName = $"_{parameterSymbol.Name}";
        if (!isAbstract && !isInterface)
        {
            return FieldDeclaration(
                    VariableDeclaration(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    parameterSymbol.Type.GetGenericDisplayName(),
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
                );
        }

        return FieldDeclaration(
                   VariableDeclaration(
                           IdentifierName(
                               Identifier(
                                   TriviaList(),
                                   parameterSymbol.Type.GetGenericDisplayName(),
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

    private static MemberDeclarationSyntax BuildBuildMethod(
        string className,
        IEnumerable<IParameterSymbol> parameterSymbols
    )
    {
        List<SyntaxNodeOrToken> list = new();
        foreach (var parameterSymbol in parameterSymbols)
        {
            list.Add(Argument(IdentifierName($"_{parameterSymbol.Name}")));
            list.Add(Token(SyntaxKind.CommaToken));
        }

        list.RemoveAt(list.Count - 1);
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

    private static MemberDeclarationSyntax WithPropertyMethod(IParameterSymbol constructorParameter, string fixtureName)
    {
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
                                                constructorParameter.Type.GetGenericDisplayName(),
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
            var primitiveName = $"{char.ToUpper(parameterSymbol.Name[0])}{parameterSymbol.Name.Substring(1, parameterSymbol.Name.Length - 1)}";
            var splitLastCamel = useParameterName(parameterSymbol) ? primitiveName : SplitLastCamel(parameterSymbol);
            return Identifier($"With{splitLastCamel}");
        }

        bool useParameterName(IParameterSymbol parameterSymbol)
        {
            return parameterSymbol.Type.TypeKind != TypeKind.Interface
             || parameterSymbol.Type.IsValueType
             || !parameterSymbol.Type.IsAbstract;
        }
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

        if (fakeItEasy is { })
        {
            return InvocationExpression(
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
            );
        }

        return InvocationExpression(
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

        TypeArgumentListSyntax typeArgumentListSyntax(IParameterSymbol parameterSymbol)
        {
            return TypeArgumentList(
                SingletonSeparatedList<TypeSyntax>(
                    ParseName(parameterSymbol.Type.GetGenericDisplayName())
                )
            );
        }
    }


    private const string Fixture = nameof(Fixture);

    private static INamedTypeSymbol? GetClassForFixture(GeneratorAttributeSyntaxContext syntaxContext)
    {
        var targetSymbol = syntaxContext.TargetSymbol as INamedTypeSymbol;

        if (syntaxContext.Attributes[0].ConstructorArguments.Length == 0)
        {
            return targetSymbol;
        }

        if (syntaxContext.Attributes[0].ConstructorArguments[0].Value is INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol;
        }

        return null;
    }


    private static bool ReportAutoFixture0001(INamedTypeSymbol classForFixture, SourceProductionContext productionContext)
    {
        if (classForFixture.Constructors.All(methodSymbol => methodSymbol.Parameters.IsDefaultOrEmpty))
        {
            return true;
        }

        return false;
    }


    private static bool ReportAutoFixture0002(INamedTypeSymbol namedTypeSymbol, SourceProductionContext productionContext)
    {
        var reported = false;
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
