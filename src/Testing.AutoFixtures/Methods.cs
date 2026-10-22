using System.Text;

using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

/// <summary>
///     Symbol helpers used by the auto fixture generator.
/// </summary>
public static class Methods
{
    /// <summary>
    ///     Gets the fully qualified metadata name of a symbol (namespace, containing types and arity).
    /// </summary>
    /// <param name="s">The symbol.</param>
    /// <returns>The metadata name, or an empty string for <see langword="null" /> and the global namespace.</returns>
    public static string GetFullMetadataName(this ISymbol? s)
    {
        if (s is null || IsRootNamespace(s))
        {
            return "";
        }

        var sb = new StringBuilder(s.MetadataName);
        var last = s;

        s = s.ContainingSymbol;

        while (!IsRootNamespace(s))
        {
            if (s is ITypeSymbol && last is ITypeSymbol)
            {
                sb.Insert(0, '+');
            }
            else
            {
                sb.Insert(0, '.');
            }

            sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            //sb.Insert(0, s.MetadataName);
            s = s.ContainingSymbol;
        }

        return sb.ToString();

        static bool IsRootNamespace(ISymbol symbol)
        {
            INamespaceSymbol? s;
            return ( s = symbol as INamespaceSymbol ) is not null && s.IsGlobalNamespace;
        }
    }

    /// <summary>
    ///     Determines whether the type is an unbound or open generic type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><see langword="true" /> when every type argument is a type parameter.</returns>
    public static bool IsOpenGenericType(this INamedTypeSymbol type) => type.IsGenericType
        && ( type.IsUnboundGenericType
            || type.TypeArguments.All(z => z.TypeKind == TypeKind.TypeParameter) );

    /// <summary>
    ///     Gets a display name for a symbol that includes its generic type arguments.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>The display name.</returns>
    public static string GetGenericDisplayName(this ISymbol? symbol)
    {
        if (symbol is null || IsRootNamespace(symbol))
        {
            return "";
        }

        var sb = new StringBuilder(symbol.MetadataName);
        if (symbol is INamedTypeSymbol namedTypeSymbol
         && ( namedTypeSymbol.IsOpenGenericType() || namedTypeSymbol.IsGenericType ))
        {
            sb = new(symbol.Name);
            if (namedTypeSymbol.IsOpenGenericType())
            {
                sb.Append('<');
                for (var i = 1; i < namedTypeSymbol.Arity - 1; i++)
                    sb.Append(',');
                sb.Append('>');
            }
            else
            {
                sb.Append('<');
                for (var index = 0; index < namedTypeSymbol.TypeArguments.Length; index++)
                {
                    var argument = namedTypeSymbol.TypeArguments[index];
                    sb.Append(GetGenericDisplayName(argument));
                    if (index < namedTypeSymbol.TypeArguments.Length - 1)
                        sb.Append(',');
                }

                sb.Append('>');
            }
        }

        var last = symbol;

        var workingSymbol = symbol.ContainingSymbol;

        if (workingSymbol is null)
            return "";

        while (!IsRootNamespace(workingSymbol))
        {
            sb.Insert(0, '.');

            sb.Insert(
                0,
                workingSymbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).Trim()
            );
            //sb.Insert(0, symbol.MetadataName);
            workingSymbol = workingSymbol.ContainingSymbol;
            last = workingSymbol;
        }

        return sb.ToString();

        static bool IsRootNamespace(ISymbol symbol)
        {
            INamespaceSymbol? s;
            return ( s = symbol as INamespaceSymbol ) is not null && s.IsGlobalNamespace;
        }
    }
}
