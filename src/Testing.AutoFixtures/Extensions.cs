using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

internal static class Extensions
{
    public static bool HasAccessibility(this IMethodSymbol symbol, params Accessibility[] accessibility)
        => accessibility.Contains(symbol.DeclaredAccessibility);

    public static bool HasPublicAccess(this ConstructorDeclarationSyntax syntax) => syntax.Modifiers.Any(token => token.IsKind(SyntaxKind.PublicKeyword));

    public static bool IsAutoFixtureAttribute(this AttributeSyntax syntax) =>
        ( (IdentifierNameSyntax)syntax.Name ).Identifier.ValueText is "AutoFixtureAttribute" or "AutoFixture";
}
