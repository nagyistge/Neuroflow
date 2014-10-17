using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace NeoComp.Computations
{
    public static class CompFormatUtils
    {
        public static InputOutputValueUnitCollection<TI, TO> ConvertToUnits<TI, TO>(CompFormatEntryCollection entries,
            ITransformer<double, TI> inputConverter,
            ITransformer<double?, TO> outputConverter)
        {
            Contract.Requires(!entries.IsNullOrEmpty());
            Contract.Requires(inputConverter != null);
            Contract.Requires(outputConverter != null);

            var result = new InputOutputValueUnitCollection<TI, TO>();

            foreach (var entry in entries)
            {
                result.Add(new InputOutputValueUnit<TI, TO>(
                    Transformation.Transform(entry.InputValues, inputConverter).ToArray(),
                    Transformation.Transform(entry.OutputValues, outputConverter).ToArray()));
            }

            return result;
        }
    }
}
