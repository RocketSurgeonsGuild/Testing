using System.Collections.Generic;
using System.Collections;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A class that can be used to create strongly typed
    /// </summary>
    public abstract class TheoryData<T> : IEnumerable<object?[]>
    {
        /// <inheritdoc />
        public IEnumerator<object?[]> GetEnumerator() => InternalGetData().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => InternalGetData().GetEnumerator();

        private IEnumerable<object?[]> InternalGetData()
        {
            var converter = TheoryDataHelpers.GetConverterMethod(typeof(T));
            foreach (var item in GetData())
            {
                yield return converter(item!);
            }
        }

        /// <summary>
        /// Gets the data from the theory
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<T> GetData();
    }
}

