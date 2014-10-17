using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    internal sealed class Population
    {
        #region Construct

        internal Population(int unitCount)
        {
            Contract.Requires(unitCount > 0);
            
            UnitContexts = new object[unitCount];
        }

        #endregion

        #region Fields And Props

        internal object[] UnitContexts { get; private set; }

        #endregion

        #region Init

        internal void Initialize(IList<OptUnit> units)
        {
            Contract.Requires(units != null);
            Contract.Requires(units.Count == UnitContexts.Length);

            for (int idx = 0; idx < UnitContexts.Length; idx++)
            {
                UnitContexts[idx] = units[idx].CreateContext();
            }
        }

        #endregion
    }
}
