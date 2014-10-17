using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    internal sealed class GenomBagComparer : IComparer<Genom>, ICloneable, IParallelInitializableGenomComprarer
    {
        internal GenomBagComparer(IComparer<Genom> baseComparer)
        {
            Contract.Requires(baseComparer != null);

            this.baseComparer = baseComparer;
        }

        IComparer<Genom> baseComparer;

        public int Compare(Genom x, Genom y)
        {
            var c = baseComparer.Compare(x, y);
            if (c == 0) c = ((IComparable<Genom>)x).CompareTo(y);
            return c;
        }

        public object Clone()
        {
            var bcc = this.baseComparer as ICloneable;
            return new GenomBagComparer(bcc == null ? this.baseComparer : (IComparer<Genom>)bcc.Clone());
        }

        public void InitializeGenomComparing(Genom g)
        {
            var bic = baseComparer as IParallelInitializableGenomComprarer;
            bic.InitializeGenomComparing(g);
        }
    }
}
