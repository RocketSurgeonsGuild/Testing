using System.Collections.Generic;

namespace Rocket.Surgery.Extensions.Testing.Fixtures
{
    /// <summary>
    /// Default methods for the <see cref="ITestFixtureBuilder"/> abstraction.
    /// </summary>
    public static class TestFixtureBuilderExtensions
    {
        /// <summary>
        /// Adds the specified field to the builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref TField field, TField value)
            where TBuilder : ITestFixtureBuilder
        {
            field = value;
            return @this;
        }

        /// <summary>
        /// Adds the specified list of fields to the builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="field">The field.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField>? field, IEnumerable<TField> values)
            where TBuilder : ITestFixtureBuilder
        {
            if (values == null)
            {
                field = null;
            }
            else if (field != null)
            {
                field.AddRange(values);
            }

            return @this;
        }

        /// <summary>
        /// Adds the specified field to the builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField>? field, TField value)
            where TBuilder : ITestFixtureBuilder
        {
            field?.Add(value);
            return @this;
        }

        /// <summary>
        /// Adds the specified key value pair to the provided dictionary.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keyValuePair">The key value pair.</param>
        /// <returns></returns>
        public static TBuilder With<TBuilder, TKey, TField>(this TBuilder @this, ref Dictionary<TKey, TField> dictionary, KeyValuePair<TKey, TField> keyValuePair)
            where TBuilder : ITestFixtureBuilder
        {
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            return @this;
        }

        /// <summary>
        /// Adds the specified key and value to the provided dictionary.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static TBuilder With<TBuilder, TKey, TField>(this TBuilder @this, ref Dictionary<TKey, TField> dictionary, TKey key, TField value)
            where TBuilder : ITestFixtureBuilder
        {
            dictionary.Add(key, value);
            return @this;
        }

        /// <summary>
        /// Adds the specified dictionary to the provided dictionary.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keyValuePair">The key value pair.</param>
        /// <returns></returns>
        public static TBuilder With<TBuilder, TKey, TField>(this TBuilder @this,
            ref Dictionary<TKey, TField> dictionary,
            IDictionary<TKey, TField> keyValuePair)
        {
            dictionary = (Dictionary<TKey, TField>)keyValuePair;
            return @this;
        }
    }
}
