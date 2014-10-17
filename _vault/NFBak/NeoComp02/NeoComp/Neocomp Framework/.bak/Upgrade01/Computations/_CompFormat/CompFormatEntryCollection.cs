using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public sealed class CompFormatEntryCollection : FixSizedItemCollection<CompFormatEntry>
    {
        public CompFormatEntryCollection(params CompFormatEntry[] entries)
            : base(entries)
        {
        }

        public CompFormatEntryCollection(IEnumerable<CompFormatEntry> entries)
            : base(entries)
        {
        }

        public int? InputSize
        {
            get { return IsEmpty ? default(int?) : this[0].InputValues.Length; }
        }

        public int? OutputSize
        {
            get { return IsEmpty ? default(int?) : this[0].OutputValues.Length; }
        }

        protected override void Validate(CompFormatEntry item)
        {
            if (item.InputValues.Length == 0)
            {
                var ex = new InvalidOperationException("Item input value array is empty.");
                ex.Data["item"] = item;
                throw ex;
            }
            if (item.OutputValues.Length == 0)
            {
                var ex = new InvalidOperationException("Item output value array is empty.");
                ex.Data["item"] = item;
                throw ex;
            }
            base.Validate(item);
        }

        protected override bool SizeEquals(CompFormatEntry item1, CompFormatEntry item2)
        {
            return item1.InputValues.Length == item2.InputValues.Length &&
                item1.OutputValues.Length == item2.OutputValues.Length;
        }

        #region MinMax

        public void GetMinAndMaxValueInfo(out double inputMin, out double inputMax, out double? outputMin, out double? outputMax)
        {
            if (IsEmpty) throw new InvalidOperationException("Collection is empty.");
            
            var inputs = from e in this
                         from v in e.InputValues
                         select v;

            var outputs = from e in this
                          from v in e.OutputValues
                          where v.HasValue
                          select v.Value;

            double? imin, imax, omin, omax;
            GetMinAndMax(inputs, out imin, out imax);
            GetMinAndMax(outputs, out omin, out omax);

            inputMin = imin.Value;
            inputMax = imax.Value;
            outputMin = omin;
            outputMax = omax;
        }

        private static void GetMinAndMax(IEnumerable<double> fromValues, out double? min, out double? max)
        {
            min = null;
            max = null;
            foreach (var value in fromValues)
            {
                if (!min.HasValue || value < min)
                {
                    min = value;
                }
                if (!max.HasValue || value > max)
                {
                    max = value;
                }
            }
        }

        #endregion

        #region To String

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var entry in this)
            {
                if (sb.Length != 0) sb.AppendLine();
                sb.Append(entry.ToString());
            }
            return sb.ToString();
        } 

        #endregion
    }
}
