namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

internal static class AutoFixtureBase
{
    // lang=csharp
    public const string Source = """
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;

        namespace Rocket.Surgery.Extensions.Testing.AutoFixtures
        {
            internal abstract class AutoFixtureBase { }

            internal abstract class AutoFixtureBase<TFixture> : AutoFixtureBase
                where TFixture : AutoFixtureBase
            {
                /// <summary>
                ///     Adds the specified field to the fixture.
                /// </summary>
                /// <typeparam name="TFixture">The type of the fixture.</typeparam>
                /// <typeparam name="TField">The type of the field.</typeparam>
                /// <param name="field">The field.</param>
                /// <param name="value">The value.</param>
                /// <returns>The fixture.</returns>
                /// <exception cref="InvalidOperationException">Throws if it cannot cast the fixture.</exception>
                protected TFixture With<TField>(ref TField field, TField value)
                {
                    field = value;
                    return this as TFixture ?? throw new InvalidOperationException();
                }

                /// <summary>
                ///     Adds the specified list of fields to the fixture.
                /// </summary>
                /// <typeparam name="TFixture">The type of the fixture.</typeparam>
                /// <typeparam name="TField">The type of the field.</typeparam>
                /// <param name="field">The field.</param>
                /// <param name="values">The values.</param>
                /// <returns>The fixture.</returns>
                /// <exception cref="InvalidOperationException">Throws if it cannot cast the fixture.</exception>
                protected TFixture With<TField>(ref Collection<TField>? field, IEnumerable<TField>? values)
                {
                    if (values == null)
                        field = null;
                    else if (field != null)
                        foreach (var item in values)
                        {
                            field.Add(item);
                        }

                    return this as TFixture ?? throw new InvalidOperationException();
                }

                /// <summary>
                ///     Adds the specified field to the fixture.
                /// </summary>
                /// <typeparam name="TFixture">The type of the fixture.</typeparam>
                /// <typeparam name="TField">The type of the field.</typeparam>
                /// <param name="field">The field.</param>
                /// <param name="value">The value.</param>
                /// <returns>The fixture.</returns>
                /// <exception cref="InvalidOperationException">Throws if it cannot cast the fixture.</exception>
                protected TFixture With<TField>(ref Collection<TField>? field, TField value)
                {
                    field?.Add(value);
                    return this as TFixture ?? throw new InvalidOperationException();
                }

                /// <summary>
                ///     Adds the specified list of fields to the fixture.
                /// </summary>
                /// <typeparam name="TFixture">The type of the fixture.</typeparam>
                /// <typeparam name="TField">The type of the field.</typeparam>
                /// <param name="field">The field.</param>
                /// <param name="values">The values.</param>
                /// <returns>The fixture.</returns>
                /// <exception cref="InvalidOperationException">Throws if it cannot cast the fixture.</exception>
                protected TFixture With<TField>(ref List<TField>? field, IEnumerable<TField>? values)
                {
                    if (values == null)
                        field = null;
                    else
                        field?.AddRange(values);

                    return this as TFixture ?? throw new InvalidOperationException();
                }

                /// <summary>
                ///     Adds the specified field to the fixture.
                /// </summary>
                /// <typeparam name="TFixture">The type of the fixture.</typeparam>
                /// <typeparam name="TField">The type of the field.</typeparam>
                /// <param name="field">The field.</param>
                /// <param name="value">The value.</param>
                /// <returns>The fixture.</returns>
                /// <exception cref="InvalidOperationException">Throws if it cannot cast the fixture.</exception>
                protected TFixture With<TField>(ref List<TField>? field, TField value)
                {
                    field?.Add(value);
                    return this as TFixture ?? throw new InvalidOperationException();
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
                /// <exception cref="InvalidOperationException">Throws if it cannot cast the fixture.</exception>
                protected TFixture With<TKey, TField>(
                    ref Dictionary<TKey, TField> dictionary,
                    KeyValuePair<TKey, TField> keyValuePair
                )
                    where TKey : notnull
                {
                    if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

                    dictionary.Add(keyValuePair.Key, keyValuePair.Value);
                    return this as TFixture ?? throw new InvalidOperationException();
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
                /// <exception cref="InvalidOperationException">Throws if it cannot cast the fixture.</exception>
                protected TFixture With<TKey, TField>(ref Dictionary<TKey, TField> dictionary, TKey key, TField value)
                    where TKey : notnull
                {
                    if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

                    dictionary.Add(key, value);
                    return this as TFixture ?? throw new InvalidOperationException();
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
                /// <exception cref="InvalidOperationException">Throws if it cannot cast the fixture.</exception>
                protected TFixture With<TKey, TField>(
                    ref Dictionary<TKey, TField> dictionary,
                    Dictionary<TKey, TField> keyValuePair
                )
                    where TKey : notnull
                {
                    dictionary = keyValuePair;
                    return this as TFixture ?? throw new InvalidOperationException();
                }

                /// <summary>
                ///     Adds the specified lazy field to the fixture.
                /// </summary>
                /// <param name="field">The field.</param>
                /// <param name="value">The value.</param>
                /// <typeparam name="TFixture">The type of the fixture.</typeparam>
                /// <typeparam name="TField">The type of the field.</typeparam>
                /// <returns></returns>
                /// <returns>The fixture.</returns>
                /// <exception cref="InvalidOperationException">Throws if it cannot cast the fixture.</exception>
                protected TFixture With<TField>(ref Lazy<TField> field, TField value)
                {
                    field = new(() => value);
                    return this as TFixture ?? throw new InvalidOperationException();
                }
            }
        }
        """;
}
