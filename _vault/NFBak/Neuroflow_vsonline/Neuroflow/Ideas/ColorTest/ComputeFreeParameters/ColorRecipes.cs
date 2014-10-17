using ColorTest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace ComputeFreeParameters
{
    public class ColorRecipes : Collection<ColorRecipe>
    {
        public ColorRecipes()
        {
        }

        public ColorRecipes(IEnumerable<ColorRecipe> coll) :
            base(coll.ToList())
        {
            Contract.Requires(coll != null);
        }

        public ColorRecipes(IList<ColorRecipe> list) :
            base(list)
        {
            Contract.Requires(list != null);
        }

        public ColorRecipes(string fileName)
        {
            Contract.Requires(!String.IsNullOrEmpty(fileName));

            Load(fileName);
        }

        public void Load(string fileName, bool append = false)
        {
            Contract.Requires(!String.IsNullOrEmpty(fileName));
            var recipeXml = XDocument.Load(fileName);
            if (!append) Clear();
            foreach (var fe in recipeXml.Root.Elements("file"))
            {
                var color = ToColor((string)fe.Attribute("color"));
                var components = (from sfe in fe.Element("from").Elements("file")
                                  let scolor = (string)sfe.Attribute("color")
                                  let samm = (string)sfe.Attribute("ammount")
                                  select new WeightedValue<HdrRGB>(ToColor(scolor), double.Parse(samm))).ToArray();
                Add(new ColorRecipe(color, components));
            }
        }

        private static HdrRGB ToColor(string color)
        {
            string[] parts = color.Split(',');
            return new HdrRGB(byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]));
        }
    }
}
