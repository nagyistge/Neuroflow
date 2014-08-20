using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Optimization.Quantum
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
        }

        internal void Stabilize(double rate, double strength)
        {
            Contract.Requires(rate > 0.0 && rate <= 1.0);
            Contract.Requires(strength > 0.0);

            double movement = Follow(item.State, rate);

            if (movement != 0.0)
            {
                Shrink(movement, strength);
            }
            else
            {
                Shrink(distance * rate, strength);
            }
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

            //if (distance < 0.5)
            //{
            //    Grow((0.5 - distance) * rate, strength);
            //}
            //else
            //{
            //    Follow(0.5, rate);
            //}
        }

        internal void Reset()
        {
            distance = 1.0 / 2.0;
            average = 1.0 / 2.0;
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
            distance -= distance * by * strength;
            if (distance < 0.0) distance = 0.0;
        }

        private void Grow(double by, double strength)
        {
            distance += distance * by * strength;
            if (distance > 0.5) distance = 0.5;
        }

        #endregion
    }
}
