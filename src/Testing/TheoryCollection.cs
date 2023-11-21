namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A class that can be used to create strongly typed
/// </summary>
public abstract class TheoryCollection<T> : IEnumerable<object?[]>
{
    /// <inheritdoc />
    public IEnumerator<object?[]> GetEnumerator()
    {
        return InternalGetData().GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return InternalGetData().GetEnumerator();
    }

    private IEnumerable<object?[]> InternalGetData()
    {
        var converter = TheoryCollectionHelpers.GetConverterMethod(typeof(T));
        foreach (var item in GetData())
        {
            yield return converter(item!);
        }
    }

    /// <summary>
    ///     Gets the data from the theory
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<T> GetData();
}
