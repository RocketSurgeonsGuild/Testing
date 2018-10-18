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
        public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField> field, IEnumerable<TField> values)
            where TBuilder : ITestFixtureBuilder
        {
            if (values == null)
            {
                field = null;
            }
            else
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
        public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField> field, TField value)
            where TBuilder : ITestFixtureBuilder
        {
            field.Add(value);
            return @this;
        }
    }
}
