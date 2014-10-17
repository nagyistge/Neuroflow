using ColorTest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ComputeFreeParameters
{
    public sealed class ColorRecipe
    {
        public ColorRecipe(HdrRGB color, params WeightedValue<HdrRGB>[] ingredients)
        {
            Contract.Requires(ingredients.Length > 0);

            Color = color;
            Ingredients = ingredients;
        }

        public HdrRGB Color { get; private set; }

        public WeightedValue<HdrRGB>[] Ingredients { get; private set; }
    }
}
