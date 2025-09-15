using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

internal static class Extensions
{
    public static bool HasAccessibility(this IMethodSymbol symbol, params Accessibility[] accessibility) =>
        accessibility.Contains(symbol.DeclaredAccessibility);

    public static bool HasPublicAccess(this ConstructorDeclarationSyntax syntax) => syntax.Modifiers.Any(token => token.IsKind(SyntaxKind.PublicKeyword));

    public static bool IsAutoFixture(this ClassDeclarationSyntax classDeclarationSyntax) =>
        classDeclarationSyntax.AttributeLists.Any(listSyntax => listSyntax.Attributes.Any(HasAutoFixtureAttribute))
     || classDeclarationSyntax.Identifier.Text is "AutoFixtureBase" or "AutoFixture" or "GeneratedCode" or "AutoFixtureAttribute";

    public static bool IsAutoFixtureAttribute(this AttributeSyntax syntax)
    {
        var name = GetLastIdentifier(syntax.Name);
        return name is "AutoFixtureAttribute" or "AutoFixture";
    }

    public static bool HasAutoFixtureAttribute(this AttributeSyntax syntax)
    {
        var name = GetLastIdentifier(syntax.Name);
        return name is "AutoFixtureAttribute" or "AutoFixture";
    }

    private static string GetLastIdentifier(NameSyntax nameSyntax) =>
        nameSyntax switch
        {
            IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
            QualifiedNameSyntax qualifiedName => GetLastIdentifier(qualifiedName.Right),
            AliasQualifiedNameSyntax aliasQualified => GetLastIdentifier(aliasQualified.Name),
            GenericNameSyntax genericName => genericName.Identifier.ValueText,
            _ => ""
        };
}
