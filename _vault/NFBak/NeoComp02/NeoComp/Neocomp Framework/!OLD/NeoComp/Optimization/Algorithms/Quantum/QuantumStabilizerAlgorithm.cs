using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Threading;

namespace NeoComp.Optimization.Algorithms.Quantum
{
    public class QuantumStabilizerAlgorithm
    {
        public QuantumStabilizerAlgorithm(IEnumerable<IQuantumStatedItem> items, double strength = 1.0, bool importState = true)
        {
            Contract.Requires(items != null);
            Contract.Requires(strength > 0.0);

            this.items = items.Select(i => new ItemStabilizer(i)).ToArray();
            if (this.items.Length == 0) throw new ArgumentException("Item collection is empty.", "items");
            this.strength = strength;

            if (importState) ImportState();
        }

        public QuantumStabilizerAlgorithm(IQuantumStatedItem item, double strength = 1.0, bool importState = true)
        {
            Contract.Requires(item != null);
            Contract.Requires(strength > 0.0);

            this.item = new ItemStabilizer(item);
            this.strength = strength;

            if (importState) ImportState();
        }

        ItemStabilizer[] items;

        ItemStabilizer item;

        double strength;

        public double Strength
        {
            get { return strength; }
        }

        #region Protocol

        public void SetupItems()
        {
            if (item != null)
            {
                item.Setup();
            }
            else
            {
                foreach (var i in items)
                {
                    i.Setup();
                }
            }
        }

        public void Stabilize(double rate, bool andSetup = true)
        {
            Contract.Requires(rate >= 0.0 && rate <= 1.0);

            if (rate != 0.0)
            {
                if (item != null)
                {
                    item.Stabilize(rate, strength);
                    if (andSetup) item.Setup();
                }
                else
                {
                    foreach (var i in items)
                    {
                        i.Stabilize(rate, strength);
                        if (andSetup) i.Setup();
                    }
                }
            }
        }

        public void Dissolve(double rate, bool andSetup = true)
        {
            Contract.Requires(rate >= 0.0 && rate <= 1.0);

            if (rate != 0.0)
            {
                if (item != null)
                {
                    item.Dissolve(rate, strength);
                    if (andSetup) item.Setup();
                }
                else
                {
                    foreach (var i in items)
                    {
                        i.Dissolve(rate, strength);
                        if (andSetup) i.Setup();
                    }
                }
            }
        }

        public void ImportState()
        {
            if (item != null)
            {
                item.ImportState();
            }
            else
            {
                foreach (var i in items)
                {
                    i.ImportState();
                }
            }
        }

        #endregion
    }
}
