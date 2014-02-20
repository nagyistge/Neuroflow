using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace NeoComp.Evolution.GA
{
    public class DNA<TGene>
    {
        #region Construct

        public DNA(IList<TGene> geneList)
        {
            Contract.Requires(geneList != null);
            Contract.Requires(geneList.Count != 0);

            Length = geneList.Count;
            var glRO = new ReadOnlyCollection<TGene>(geneList);
            var glROArray = new[] { glRO };
            Genes = Array.AsReadOnly(glROArray);
        }

        public DNA(IList<TGene>[] geneLists)
        {
            Contract.Requires(geneLists != null);
            Contract.Requires(geneLists.Length != 0);

            var glROArray = new ReadOnlyCollection<TGene>[geneLists.Length];
            for (int idx = 0; idx < glROArray.Length; idx++)
            {
                var genes = geneLists[idx];
                if (genes == null || genes.Count == 0) throw new ArgumentException("geneLists", "Gene sequence is null or empty.");
                if (genes.Count > Length) Length = genes.Count;
                glROArray[idx] = new ReadOnlyCollection<TGene>(genes);
            }
            Genes = Array.AsReadOnly(glROArray);
        }

        #endregion

        #region Props and Fields

        public ReadOnlyCollection<ReadOnlyCollection<TGene>> Genes { get; private set; }

        public int Length { get; private set; }

        #endregion
    }
}
