using System;

namespace ExtremeAndy.CombinatoryFilters
{
    /// <summary>
    /// Don't try to synchronise between threads: if the value is ready, return it, otherwise call the factory.
    /// Significantly faster than <see cref="Lazy{T}"/>.
    /// </summary>
    internal struct SimpleLazy<T>
    {
        private volatile Func<T> _valueFactory;
        private T _value;

        public SimpleLazy(Func<T> valueFactory)
        {
            _valueFactory = valueFactory;
            _value = default;
            IsValueCreated = false;
        }

        public volatile bool IsValueCreated;

        public T Value
        {
            get
            {
                if (!IsValueCreated)
                {
                    var valueFactory = _valueFactory;

                    // While we were fetching valueFactory from the field, another thread
                    // may have finished writing the value, and set the factory to null.
                    // In this case, we can exit early.
                    if (valueFactory == null)
                    {
                        return _value;
                    }

                    _value = valueFactory();
                    IsValueCreated = true;

                    // Allow GC to clean up any resources related to the valueFactory.
                    _valueFactory = null;
                }

                return _value;
            }
        }
    }
}
