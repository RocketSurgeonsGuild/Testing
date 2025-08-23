namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

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