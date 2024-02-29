using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.Extensions.Testing.Fixtures.SourceGenerator;

[Generator]
public class AutoFixtureGenerator : IIncrementalGenerator //, ISourceGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider =
            context
               .SyntaxProvider
               .ForAttributeWithMetadataName(
                    "AutoFixtureAttribute",
                    (node, token) => node.IsKind(SyntaxKind.ClassDeclaration),
                    (syntaxContext, token) => syntaxContext
                )
               .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(syntaxProvider, GenerateFixtureBuilder);

        // do generator things
        context.RegisterPostInitializationOutput(
            initializationContext =>
            {
                initializationContext.AddSource(nameof(Attribute), Attribute);
                initializationContext.AddSource(nameof(BuilderExtensions), BuilderExtensions);
            }
        );

        void GenerateFixtureBuilder(
            SourceProductionContext productionContext,
            (GeneratorAttributeSyntaxContext context, Compilation compilation) valueTuple
        )
        {
            var (syntaxContext, compilation) = valueTuple;

            compilation.GetTypeByMetadataName("NSubstitute.Substitute");

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

            var fullList =
                new[] { Operator(namedTypeSymbol) }
                   .Concat(parameterSymbols.Select(symbol => WithMethod(symbol)))
                   .Concat(parameterSymbols.Select(symbol => BuildFields(symbol)));

            var classDeclaration = BuildClassDeclaration(namedTypeSymbol)
               .WithMembers(new SyntaxList<MemberDeclarationSyntax>(fullList));

            var namespaceDeclaration = BuildNamespace(namedTypeSymbol)
               .WithMembers(new SyntaxList<MemberDeclarationSyntax>(classDeclaration));

            var usings = new SyntaxList<UsingDirectiveSyntax>(
                parameterSymbols
                   .Distinct(new ParameterSymbolNamespaceComparer())
                   .Select(parameterSymbol => BuildUsing(parameterSymbol))
            );

            var unit =
                CompilationUnit()
                   .WithUsings(usings)
                   .AddUsings(BuildUsing("NSubstitute"))
                   .AddMembers(namespaceDeclaration)
                   .NormalizeWhitespace();

            productionContext.AddSource("AutoFixture", unit.ToFullString());
        }
    }

    private static UsingDirectiveSyntax BuildUsing(string name) =>
        UsingDirective(IdentifierName(name))
           .WithUsingKeyword(Token(TriviaList(), SyntaxKind.UsingKeyword, TriviaList(Space)))
           .WithTrailingTrivia(LineFeed);

    private static NamespaceDeclarationSyntax BuildNamespace(ISymbol namedTypeSymbol) =>
        NamespaceDeclaration(
                ParseName(namedTypeSymbol.ContainingNamespace.ToDisplayString())
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

    private static ClassDeclarationSyntax BuildClassDeclaration(ISymbol namedTypeSymbol) =>
        ClassDeclaration(
                Identifier(
                    TriviaList(),
                    $"{namedTypeSymbol.Name}{Fixture}",
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
                            SyntaxKind.PartialKeyword,
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
                        )
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
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        TestFixtureBuilder,
                                        TriviaList(
                                            LineFeed
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

    private static MemberDeclarationSyntax BuildFields(IParameterSymbol parameterSymbol)
    {
        return FieldDeclaration(
                   VariableDeclaration(
                           IdentifierName(
                               Identifier(
                                   TriviaList(),
                                   parameterSymbol.Type.Name,
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
                                               InvocationExpression(
                                                   MemberAccessExpression(
                                                       SyntaxKind.SimpleMemberAccessExpression,
                                                       IdentifierName("Substitute"),
                                                       GenericName(
                                                               Identifier("For")
                                                           )
                                                          .WithTypeArgumentList(
                                                               TypeArgumentList(
                                                                   SingletonSeparatedList<TypeSyntax>(
                                                                       IdentifierName(parameterSymbol.Type.Name)
                                                                   )
                                                               )
                                                           )
                                                   )
                                               )
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

    private static MemberDeclarationSyntax WithMethod(IParameterSymbol constructorParameter)
    {
        var methodName = SplitLastCamel(constructorParameter);
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
                    Identifier($"With{methodName}")
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
                                                constructorParameter.Type.Name,
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
                                        ThisExpression(),
                                        IdentifierName("With")
                                    )
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
                                                )
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

    private static MemberDeclarationSyntax Operator(ISymbol namedTypeSymbol) =>
        ConversionOperatorDeclaration(
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
                        )
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

    private static UsingDirectiveSyntax BuildUsing(IParameterSymbol parameterSymbol)
    {
        var identifiers = parameterSymbol.Type.ContainingNamespace.ToDisplayParts()
                                         .Where(x => x.Kind != SymbolDisplayPartKind.Punctuation)
                                         .Select(x => IdentifierName(x.ToString())).ToList();

        if (identifiers.Count == 1)
        {
            return UsingDirective(identifiers[0])
                  .WithUsingKeyword(Token(TriviaList(), SyntaxKind.UsingKeyword, TriviaList(Space)))
                  .WithTrailingTrivia(LineFeed);
        }

        return UsingDirective(ParseName(parameterSymbol.ContainingNamespace.ToDisplayString()))
              .WithUsingKeyword(Token(TriviaList(), SyntaxKind.UsingKeyword, TriviaList(Space)))
              .WithTrailingTrivia(LineFeed);
    }

    private static string SplitLastCamel(IParameterSymbol typeSymbol) =>
        System.Text.RegularExpressions.Regex.Replace(
            typeSymbol.Type.Name, "([A-Z])", " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled
        ).Trim().Split(' ').Last();

    private const string Fixture = nameof(Fixture);

    private const string TestFixtureBuilder = "ITestFixtureBuilder";

    private const string Attribute = @"using System;
using System.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.AutoFixture;

[AttributeUsage(AttributeTargets.Class)]
[Conditional(""CODEGEN"")]
internal class AutoFixtureAttribute : Attribute
{
    public AutoFixtureAttribute(Type type) => Type = type;

    public Type Type { get; }
}";

    private const string BuilderExtensions = @"using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Rocket.Surgery.Extensions.Testing.AutoFixture;

/// <summary>
///     for the <see cref=""TestFixtureBuilderExtensions"" /> extension methods.
/// </summary>
public interface ITestFixtureBuilder;

/// <summary>
///     Default methods for the <see cref=""ITestFixtureBuilder"" /> abstraction.
/// </summary>
internal static class TestFixtureBuilderExtensions
{
    /// <summary>
    ///     Adds the specified field to the builder.
    /// </summary>
    /// <typeparam name=""TBuilder"">The type of the builder.</typeparam>
    /// <typeparam name=""TField"">The type of the field.</typeparam>
    /// <param name=""this"">The this.</param>
    /// <param name=""field"">The field.</param>
    /// <param name=""value"">The value.</param>
    /// <returns></returns>
    // ReSharper disable once RedundantAssignment
    public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref TField field, TField value)
        where TBuilder : ITestFixtureBuilder
    {
        field = value;
        return @this;
    }

    /// <summary>
    ///     Adds the specified list of fields to the builder.
    /// </summary>
    /// <typeparam name=""TBuilder"">The type of the builder.</typeparam>
    /// <typeparam name=""TField"">The type of the field.</typeparam>
    /// <param name=""this"">The this.</param>
    /// <param name=""field"">The field.</param>
    /// <param name=""values"">The values.</param>
    /// <returns></returns>
    public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref Collection<TField>? field, IEnumerable<TField>? values)
        where TBuilder : ITestFixtureBuilder
    {
        if (values == null)
        {
            field = null;
        }
        else if (field != null)
        {
            foreach (var item in values)
                field.Add(item);
        }

        return @this;
    }

    /// <summary>
    ///     Adds the specified list of fields to the builder.
    /// </summary>
    /// <typeparam name=""TBuilder"">The type of the builder.</typeparam>
    /// <typeparam name=""TField"">The type of the field.</typeparam>
    /// <param name=""this"">The this.</param>
    /// <param name=""field"">The field.</param>
    /// <param name=""values"">The values.</param>
    /// <returns></returns>
#pragma warning disable CA1002
    public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField>? field, IEnumerable<TField>? values)
#pragma warning restore CA1002
        where TBuilder : ITestFixtureBuilder
    {
        if (values == null)
        {
            field = null;
        }
        else if (field is not null)
        {
            field.AddRange(values);
        }

        return @this;
    }

    /// <summary>
    ///     Adds the specified field to the builder.
    /// </summary>
    /// <typeparam name=""TBuilder"">The type of the builder.</typeparam>
    /// <typeparam name=""TField"">The type of the field.</typeparam>
    /// <param name=""this"">The this.</param>
    /// <param name=""field"">The field.</param>
    /// <param name=""value"">The value.</param>
    /// <returns></returns>
    public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref Collection<TField>? field, TField value)
        where TBuilder : ITestFixtureBuilder
    {
        field?.Add(value);
        return @this;
    }

    /// <summary>
    ///     Adds the specified field to the builder.
    /// </summary>
    /// <typeparam name=""TBuilder"">The type of the builder.</typeparam>
    /// <typeparam name=""TField"">The type of the field.</typeparam>
    /// <param name=""this"">The this.</param>
    /// <param name=""field"">The field.</param>
    /// <param name=""value"">The value.</param>
    /// <returns></returns>
#pragma warning disable CA1002
    public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField>? field, TField value)
#pragma warning restore CA1002
        where TBuilder : ITestFixtureBuilder
    {
        field?.Add(value);
        return @this;
    }

    /// <summary>
    ///     Adds the specified key value pair to the provided dictionary.
    /// </summary>
    /// <typeparam name=""TBuilder"">The type of the builder.</typeparam>
    /// <typeparam name=""TKey"">The type of the key.</typeparam>
    /// <typeparam name=""TField"">The type of the field.</typeparam>
    /// <param name=""this"">The this.</param>
    /// <param name=""dictionary"">The dictionary.</param>
    /// <param name=""keyValuePair"">The key value pair.</param>
    /// <returns></returns>
    public static TBuilder With<TBuilder, TKey, TField>(
        this TBuilder @this, ref Dictionary<TKey, TField> dictionary, KeyValuePair<TKey, TField> keyValuePair
    )
        where TBuilder : ITestFixtureBuilder
        where TKey : notnull
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        return @this;
    }

    /// <summary>
    ///     Adds the specified key and value to the provided dictionary.
    /// </summary>
    /// <typeparam name=""TBuilder"">The type of the builder.</typeparam>
    /// <typeparam name=""TKey"">The type of the key.</typeparam>
    /// <typeparam name=""TField"">The type of the field.</typeparam>
    /// <param name=""this"">The this.</param>
    /// <param name=""dictionary"">The dictionary.</param>
    /// <param name=""key"">The key.</param>
    /// <param name=""value"">The value.</param>
    /// <returns></returns>
    public static TBuilder With<TBuilder, TKey, TField>(this TBuilder @this, ref Dictionary<TKey, TField> dictionary, TKey key, TField value)
        where TBuilder : ITestFixtureBuilder
        where TKey : notnull
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        dictionary.Add(key, value);
        return @this;
    }

    /// <summary>
    ///     Adds the specified dictionary to the provided dictionary.
    /// </summary>
    /// <typeparam name=""TBuilder"">The type of the builder.</typeparam>
    /// <typeparam name=""TKey"">The type of the key.</typeparam>
    /// <typeparam name=""TField"">The type of the field.</typeparam>
    /// <param name=""this"">The this.</param>
    /// <param name=""dictionary"">The dictionary.</param>
    /// <param name=""keyValuePair"">The key value pair.</param>
    /// <returns></returns>
    public static TBuilder With<TBuilder, TKey, TField>(
        this TBuilder @this,
        // ReSharper disable once RedundantAssignment
        ref Dictionary<TKey, TField> dictionary,
        Dictionary<TKey, TField> keyValuePair
    )
        where TKey : notnull
    {
        dictionary = keyValuePair;
        return @this;
    }
}";

    private class ParameterSymbolNamespaceComparer : IEqualityComparer<IParameterSymbol>
    {
        public bool Equals(IParameterSymbol x, IParameterSymbol y)
        {
            return SymbolEqualityComparer.Default.Equals(x.Type.ContainingNamespace, y.Type.ContainingNamespace);
        }

        public int GetHashCode(IParameterSymbol obj)
        {
            return SymbolEqualityComparer.Default.GetHashCode(obj.Type.ContainingNamespace);
        }
    }
}