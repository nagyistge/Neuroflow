using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public class WeightedValue<T>
    {
        public WeightedValue(T value, double weight)
        {
            this.value = value;
            Weight = weight;
        }

        T value;

        public T Value
        {
            get { return value; }
        }

        public double Weight { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1}", value, Weight.ToString("0.00"));
        }
    }
}
