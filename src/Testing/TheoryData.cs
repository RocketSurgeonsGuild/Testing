using System.Collections.Generic;
using System.Collections;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A class that can be used to create strongly typed
    /// </summary>
    public abstract class TheoryData<T> : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator() => InternalGetData().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => InternalGetData().GetEnumerator();

        private IEnumerable<object[]> InternalGetData()
        {
            var _converter = TheoryDataHelpers.GetConverterMethod(typeof(T));
            foreach (var item in GetData())
            {
                yield return _converter(item);
            }
        }

        protected abstract IEnumerable<T> GetData();
    }
}

