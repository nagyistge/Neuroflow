using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Optimization.Algorithms.Quantum
{
    internal sealed class ItemStabilizer
    {
        public ItemStabilizer(IQuantumStatedItem item)
        {
            Contract.Requires(item != null);
            this.item = item;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(distance >= 0.0 && distance <= 0.5);
            Contract.Invariant(average >= 0.0 && average <= 1.0);
        }
        
        readonly IQuantumStatedItem item;

        double distance = 1.0 / 2.0;

        double average = 1.0 / 2.0;

        #region Work

        internal void Setup()
        {
            double min = average - distance;
            double max = average + distance;
            double d = max - min;
            item.State = RandomGenerator.Random.NextDouble() * d + min;
            //double gauss = Statistics.GenerateGauss(average, distance);
            //item.State = gauss;
        }

        internal void ImportState()
        {
            average = item.State;
            distance = 0.5;
        }

        internal void Stabilize(double rate, double strength)
        {
            Contract.Requires(rate > 0.0 && rate <= 1.0);
            Contract.Requires(strength > 0.0);

            double movement = Follow(item.State, rate);

            if (movement < 0.0000000001) movement = 0.0000000001;

            Shrink(movement, strength);
        }

        internal void Dissolve(double rate, double strength)
        {
            Contract.Requires(rate > 0.0 && rate <= 1.0);
            Contract.Requires(strength > 0.0);

            if (distance < 0.5)
            {
                Grow((0.5 - distance) * rate, strength);
            }

            double d2 = distance / 2.0;
            if ((average + d2) > 1.0 || (average - d2) < 0.0)
            {
                Follow(0.5, rate);
            }
        }

        private double Follow(double to, double rate)
        {
            double movement = 0.0;
            if (to < average)
            {
                movement = (average - to) * rate;
                average -= movement;
            }
            else if (to > average)
            {
                movement = (to - average) * rate;
                average += movement;
            }
            return movement;
        }

        private void Shrink(double by, double strength)
        {
            distance -= distance * strength * by;
            if (distance < 0.0) distance = 0.0;
        }

        private void Grow(double by, double strength)
        {
            distance += distance * strength * by;
            if (distance > 0.5) distance = 0.5;
        }

        #endregion
    }
}
