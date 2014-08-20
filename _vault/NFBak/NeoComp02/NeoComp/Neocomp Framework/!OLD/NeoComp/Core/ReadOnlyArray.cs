using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Core
{
    public static class ReadOnlyArray
    {
        public static ReadOnlyArray<T> Wrap<T>(params T[] items)
        {
            return new ReadOnlyArray<T>(items, true);
        }
    }
    
    public class ReadOnlyArray<T> : ReadOnlyCollection<T>
    {
        #region Constructors

        public ReadOnlyArray()
            : base(new T[0])
        {
        }
        
        public ReadOnlyArray(params T[] items)
            : this(items, false)
        {
        }

        protected internal ReadOnlyArray(T[] items, bool wrap)
            : base(items == null ? new T[0] : (wrap ? items : items.ToArray()))
        {
        }

        public ReadOnlyArray(IEnumerable<T> items)
            : base(items.ToArray())
        {
            Contract.Requires(items != null);
        }

        public ReadOnlyArray(IList<T> items)
            : base(items.ToArray())
        {
            Contract.Requires(items != null);
        } 

        #endregion

        #region Properties

        [Pure]
        protected internal T[] ItemArray
        {
            get { return (T[])base.Items; }
        }

        public bool IsEmpty
        {
            [Pure]
            get { return ItemArray.Length == 0; }
        } 

        #endregion

        #region ToString

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var value in ItemArray)
            {
                if (sb.Length != 0) sb.Append(", ");
                if (value == null) sb.Append("null"); else sb.Append(value);
            }
            return sb.ToString();
        }

        #endregion
    }
}
