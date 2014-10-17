using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Threading;

namespace NeoComp.Optimization.Quantum
{
    public class QuantumStabilizerAlgorithm
    {
        public QuantumStabilizerAlgorithm(IEnumerable<IQuantumStatedItem> items, double strength = 1.0)
        {
            Contract.Requires(items != null);
            Contract.Requires(strength > 0.0);

            this.items = items.Select(i => new ItemStabilizer(i)).ToArray();
            if (this.items.Length == 0) throw new ArgumentException("Item collection is empty.", "items");
            this.strength = strength;
        }

        ItemStabilizer[] items;

        double strength;

        public double Strength
        {
            get { return strength; }
            set
            {
                Contract.Requires(value > 0.0);

                strength = value;
            }
        }

        #region Protocol

        public void SetupItems()
        {
            foreach (var item in items)
            {
                item.Setup();
            }
        }

        public void Stabilize(double rate, bool andSetup = false)
        {
            //Contract.Requires(rate >= 0.0 && rate <= 1.0);

            if (rate != 0.0)
            {
                foreach (var item in items)
                {
                    item.Stabilize(rate, strength);
                    if (andSetup) item.Setup();
                }
                //Parallel.For(0, items.Length, (idx) =>
                //{
                //    var item = items[idx];
                //    item.Stabilize(rate, strength);
                //    if (andSetup) item.Setup();
                //});
            }
        }

        public void Dissolve(double rate, bool andSetup = false)
        {
           // Contract.Requires(rate >= 0.0 && rate <= 1.0);

            if (rate != 0.0)
            {
                foreach (var item in items)
                {
                    item.Dissolve(rate, strength);
                    if (andSetup) item.Setup();
                }
                //Parallel.For(0, items.Length, (idx) =>
                //{
                //    var item = items[idx];
                //    item.Dissolve(rate, strength);
                //    if (andSetup) item.Setup();
                //});
            }
        }

        public void Reset()
        {
            foreach (var item in items)
            {
                item.Reset();
            }
        }

        #endregion
    }
}
