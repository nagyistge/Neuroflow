using ColorTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputeFreeParameters
{
    public class GeneBasedColorMixer
    {
        public GeneBasedColorMixer(IEnumerable<ColorRecipe> recipes)
        {
            Contract.Requires(recipes != null);

            Recipes = recipes as ColorRecipes;
            if (Recipes == null) Recipes = new ColorRecipes(recipes);
        }

        public ColorRecipes Recipes { get; private set; }

        public MixedColorsResult CreateMixedColors(Genom genom)
        {
            ColorFilteringPars filteringPars;
            ColorMixingPars mixingPars;
            ToColorMixerPars(genom, out filteringPars, out mixingPars);

            return new MixedColorsResult(
                from recipe in Recipes.AsParallel()
                let mixed = ColorMixer.Mix(filteringPars, mixingPars, recipe.Ingredients)
                select new MixedColor(recipe.Color, mixed));
        }

        public static void ToColorMixerPars(Genom genom, out ColorFilteringPars filteringPars, out ColorMixingPars mixingPars)
        {
            Contract.Requires(genom != null);

            filteringPars = new ColorFilteringPars();
            mixingPars = new ColorMixingPars();
            
            int geneIndex = 0;
            filteringPars.Init(genom.Genes, ref geneIndex);
            mixingPars.Init(genom.Genes, ref geneIndex);
            Debug.Assert(geneIndex == genom.Genes.Length);
        }
    }
}
