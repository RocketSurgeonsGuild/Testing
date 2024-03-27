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

    private static ClassDeclarationSyntax BuildClassDeclaration(ISymbol namedTypeSymbol)
    {
        var fixture = $"{namedTypeSymbol.Name}{Fixture}";

        return ClassDeclaration(
                   Identifier(
                       TriviaList(),
                       fixture,
                       TriviaList(
                           Space
                       )
                   )
               )
              .WithModifiers(
                   TokenList(
                       [
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
                           ),
                       ]
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
                                   ).WithTypeArgumentList(
                                       TypeArgumentList(
                                           SingletonSeparatedList<TypeSyntax>(
                                               IdentifierName(fixture))))
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
    }

    private static MemberDeclarationSyntax BuildFields(
        IParameterSymbol parameterSymbol,
        InvocationExpressionSyntax invocationExpressionSyntax
    )
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
                                       Identifier(
                                           TriviaList(),
                                           $"_{parameterSymbol.Name}",
                                           TriviaList(
                                               Space
                                           )
                                       )
                                   )
                                  .WithInitializer(
                                       EqualsValueClause(
                                               // TODO: [rlittlesii: February 29, 2024] Replace with FakeItEasy
                                               invocationExpressionSyntax
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
        ISymbol namedTypeSymbol,
        IEnumerable<IParameterSymbol> parameterSymbols
    )
    {
        var list = new List<SyntaxNodeOrToken>();
        foreach (var parameterSymbol in parameterSymbols)
        {
            list.Add(Argument(IdentifierName($"_{parameterSymbol.Name}")));
            list.Add(Token(SyntaxKind.CommaToken));
        }

        list.RemoveAt(list.Count - 1);
        return GlobalStatement(
            LocalFunctionStatement(
                    IdentifierName(namedTypeSymbol.Name),
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
                                IdentifierName(namedTypeSymbol.Name)
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

    private static MemberDeclarationSyntax WithPropertyMethod(IParameterSymbol constructorParameter)
    {
        return GlobalStatement(
            LocalFunctionStatement(
                    IdentifierName(
                        Identifier(
                            TriviaList(),
                            $"{constructorParameter.ContainingType.Name}{Fixture}",
                            TriviaList(
                                Space
                            )
                        )
                    ),
                    Identifier($"With{SplitLastCamel(constructorParameter)}")
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
    }

    private static MemberDeclarationSyntax Operator(ISymbol namedTypeSymbol)
    {
        return ConversionOperatorDeclaration(
                   Token(
                       TriviaList(),
                       SyntaxKind.ImplicitKeyword,
                       TriviaList(
                           Space
                       )
                   ),
                   IdentifierName(namedTypeSymbol.Name)
               )
              .WithModifiers(
                   TokenList(
                       [
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
                           ),
                       ]
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
                                               $"{namedTypeSymbol.Name}{Fixture}",
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
    }

    private static string SplitLastCamel(IParameterSymbol typeSymbol)
    {
        return Regex
              .Replace(typeSymbol.Type.Name, "([A-Z])", " $1", RegexOptions.Compiled)
              .Trim()
              .Split(' ')
              .Last();
    }

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
                            TypeArgumentListSyntax(symbol)
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
                        TypeArgumentListSyntax(
                            symbol
                        )
                    )
            )
        );

        TypeArgumentListSyntax TypeArgumentListSyntax(IParameterSymbol parameterSymbol)
        {
            return TypeArgumentList(
                SingletonSeparatedList<TypeSyntax>(
                    ParseName(parameterSymbol.Type.GetGenericDisplayName())
                )
            );
        }
    }

    private const string Fixture = nameof(Fixture);

//    private const string TestFixtureBuilder = "ITestFixtureBuilder";
}
