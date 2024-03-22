using System.Collections.ObjectModel;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

internal abstract class AutoFixtureBase
{
    /// <summary>
    ///     Adds the specified field to the fixture.
    /// </summary>
    /// <typeparam name="TFixture">The type of the fixture.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    /// <param name="field">The field.</param>
    /// <param name="value">The value.</param>
    /// <returns>The fixture.</returns>
    protected TFixture With<TFixture, TField>(ref TField field, TField value)
        where TFixture : AutoFixtureBase
    {
        field = value;
        return (TFixture)this;
    }

    /// <summary>
    ///     Adds the specified list of fields to the fixture.
    /// </summary>
    /// <typeparam name="TFixture">The type of the fixture.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    /// <param name="field">The field.</param>
    /// <param name="values">The values.</param>
    /// <returns>The fixture.</returns>
    protected TFixture With<TFixture, TField>(ref Collection<TField>? field, IEnumerable<TField>? values)
        where TFixture : AutoFixtureBase
    {
        if (values == null)
            field = null;
        else if (field != null)
            foreach (var item in values)
            {
                field.Add(item);
            }

        return (TFixture)this;
    }

    /// <summary>
    ///     Adds the specified list of fields to the fixture.
    /// </summary>
    /// <typeparam name="TFixture">The type of the fixture.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    /// <param name="field">The field.</param>
    /// <param name="values">The values.</param>
    /// <returns>The fixture.</returns>
    protected TFixture With<TFixture, TField>(ref List<TField>? field, IEnumerable<TField>? values)
        where TFixture : AutoFixtureBase
    {
        if (values == null)
            field = null;
        else
            field?.AddRange(values);

        return (TFixture)this;
    }

    /// <summary>
    ///     Adds the specified field to the fixture.
    /// </summary>
    /// <typeparam name="TFixture">The type of the fixture.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    /// <param name="field">The field.</param>
    /// <param name="value">The value.</param>
    /// <returns>The fixture.</returns>
    protected TFixture With<TFixture, TField>(ref Collection<TField>? field, TField value)
        where TFixture : AutoFixtureBase
    {
        field?.Add(value);
        return (TFixture)this;
    }

    /// <summary>
    ///     Adds the specified field to the fixture.
    /// </summary>
    /// <typeparam name="TFixture">The type of the fixture.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    /// <param name="field">The field.</param>
    /// <param name="value">The value.</param>
    /// <returns>The fixture.</returns>
    #pragma warning disable CA1002
    protected TFixture With<TFixture, TField>(ref List<TField>? field, TField value)
        #pragma warning restore CA1002
        where TFixture : AutoFixtureBase
    {
        field?.Add(value);
        return (TFixture)this;
    }

    /// <summary>
    ///     Adds the specified key value pair to the provided dictionary.
    /// </summary>
    /// <typeparam name="TFixture">The type of the fixture.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="keyValuePair">The key value pair.</param>
    /// <returns>The fixture.</returns>
    protected TFixture With<TFixture, TKey, TField>(
        ref Dictionary<TKey, TField> dictionary,
        KeyValuePair<TKey, TField> keyValuePair
    )
        where TFixture : AutoFixtureBase
        where TKey : notnull
    {
        if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

        dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        return (TFixture)this;
    }

    /// <summary>
    ///     Adds the specified key and value to the provided dictionary.
    /// </summary>
    /// <typeparam name="TFixture">The type of the fixture.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>The fixture.</returns>
    protected TFixture With<TFixture, TKey, TField>(ref Dictionary<TKey, TField> dictionary, TKey key, TField value)
        where TFixture : AutoFixtureBase
        where TKey : notnull
    {
        if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

        dictionary.Add(key, value);
        return (TFixture)this;
    }

    /// <summary>
    ///     Adds the specified dictionary to the provided dictionary.
    /// </summary>
    /// <typeparam name="TFixture">The type of the fixture.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="keyValuePair">The key value pair.</param>
    /// <returns>The fixture.</returns>
    protected TFixture With<TFixture, TKey, TField>(
        ref Dictionary<TKey, TField> dictionary,
        Dictionary<TKey, TField> keyValuePair
    )
        where TFixture : AutoFixtureBase
        where TKey : notnull
    {
        dictionary = keyValuePair;
        return (TFixture)this;
    }
}

internal partial class DeckFixture : AutoFixtureBase
{
    private int _period;
    public DeckFixture WithStuff(int period) => With<DeckFixture, int>(ref _period, period);
}
