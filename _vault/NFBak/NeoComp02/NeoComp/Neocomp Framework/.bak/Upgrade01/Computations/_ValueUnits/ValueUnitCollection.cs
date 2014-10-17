using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public class ValueUnitCollection<T> : FixSizedItemCollection<T[]>
    {
        public ValueUnitCollection(params T[][] units)
            : base(units)
        {
        }

        public ValueUnitCollection(IEnumerable<T[]> units)
            : base(units)
        {
        }

        public int? Size
        {
            get { return IsEmpty ? default(int?) : this[0].Length; }
        }

        protected override void Validate(T[] item)
        {
            if (item.Length == 0)
            {
                var ex = new InvalidOperationException("Item array is empty.");
                throw ex;
            }
            base.Validate(item);
        }

        protected override bool SizeEquals(T[] item1, T[] item2)
        {
            return item1.Length == item2.Length;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var unit in this)
            {
                if (sb.Length != 0) sb.AppendLine();
                sb.AppendValues(unit);
            }
            return sb.ToString();
        }
    }
}
