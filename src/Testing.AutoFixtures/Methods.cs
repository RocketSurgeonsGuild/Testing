using System.Text;
using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

public static class Methods
{
    public static string GetFullMetadataName(this ISymbol? s)
    {
        if (s == null || IsRootNamespace(s))
        {
            return string.Empty;
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
            return ( s = symbol as INamespaceSymbol ) != null && s.IsGlobalNamespace;
        }
    }

    public static bool IsOpenGenericType(this INamedTypeSymbol type)
    {
        return type.IsGenericType
         && ( type.IsUnboundGenericType
             || type.TypeArguments.All(z => z.TypeKind == TypeKind.TypeParameter) );
    }

    public static string GetGenericDisplayName(this ISymbol? symbol)
    {
        if (symbol == null || IsRootNamespace(symbol))
        {
            return string.Empty;
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
            return string.Empty;

        while (!IsRootNamespace(workingSymbol))
        {
            if (workingSymbol is ITypeSymbol && last is ITypeSymbol)
            {
                sb.Insert(0, '+');
            }
            else
            {
                sb.Insert(0, '.');
            }

            sb.Insert(
                0,
                workingSymbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).Trim()
            );
            //sb.Insert(0, symbol.MetadataName);
            workingSymbol = workingSymbol.ContainingSymbol;
        }

        return sb.ToString();

        static bool IsRootNamespace(ISymbol symbol)
        {
            INamespaceSymbol? s;
            return ( s = symbol as INamespaceSymbol ) != null && s.IsGlobalNamespace;
        }
    }
}