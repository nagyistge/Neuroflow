using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public class InputOutputValueUnitCollection<TI, TO> : FixSizedItemCollection<InputOutputValueUnit<TI, TO>>
    {
        public InputOutputValueUnitCollection(params InputOutputValueUnit<TI, TO>[] units)
            : base(units)
        {
        }

        public InputOutputValueUnitCollection(IEnumerable<InputOutputValueUnit<TI, TO>> units)
            : base(units)
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

        protected override void Validate(InputOutputValueUnit<TI, TO> item)
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

        protected override bool SizeEquals(InputOutputValueUnit<TI, TO> item1, InputOutputValueUnit<TI, TO> item2)
        {
            return item1.InputValues.Length == item2.InputValues.Length &&
                item1.OutputValues.Length == item2.OutputValues.Length;
        }

        public ValueUnitCollection<TI> GetInputValueUnits()
        {
            return new ValueUnitCollection<TI>(this.Select(u => u.InputValues));
        }

        public ValueUnitCollection<TO> GetOutputValueUnits()
        {
            return new ValueUnitCollection<TO>(this.Select(u => u.OutputValues));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var unit in this)
            {
                if (sb.Length != 0) sb.AppendLine();
                sb.Append(unit.ToString());
            }
            return sb.ToString();
        }
    }
}
