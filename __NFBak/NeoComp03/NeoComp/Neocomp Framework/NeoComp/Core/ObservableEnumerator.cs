using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Core
{
    public sealed class ObservableEnumerator<T> : IEnumerator<T>
    {
        public ObservableEnumerator(IEnumerable<T> enumerable)
            : this(enumerable.GetEnumerator())
        {
        }

        public ObservableEnumerator(IEnumerator<T> enumerator)
        {
            Contract.Requires(enumerator != null);

            this.enumerator = enumerator;
            onEnd = false;
        }

        IEnumerator<T> enumerator;

        bool onEnd;

        internal bool OnEnd
        {
            get { return onEnd; }
        }

        #region IEnumerator<T> Members

        public T Current
        {
            get { return enumerator.Current; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get { return enumerator.Current; }
        }

        public bool MoveNext()
        {
            bool next = enumerator.MoveNext();
            onEnd = !next;
            return next;
        }

        public void Reset()
        {
            enumerator.Reset();
        }

        #endregion
    }
}
