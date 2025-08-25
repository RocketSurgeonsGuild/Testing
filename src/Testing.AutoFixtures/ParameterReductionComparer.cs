using Microsoft.CodeAnalysis;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

internal class ParameterReductionComparer : IEqualityComparer<IParameterSymbol>
{
    public bool Equals(IParameterSymbol x, IParameterSymbol y) =>
        ( x.Type.Equals(y.Type) && x.Name.Equals(y.Name) ) || SymbolEqualityComparer.Default.Equals(x, y);

    public int GetHashCode(IParameterSymbol obj) => SymbolEqualityComparer.Default.GetHashCode(obj.Type) + obj.Type.GetHashCode() + obj.Name.GetHashCode();
    public static IEqualityComparer<IParameterSymbol> Default { get; } = new ParameterReductionComparer();
}
