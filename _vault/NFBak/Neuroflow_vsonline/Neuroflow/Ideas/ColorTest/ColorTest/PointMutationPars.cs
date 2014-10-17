using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public enum PointMutationType { Uniform, Gaussian, Reset }

    public struct PointMutationPars
    {
        public PointMutationPars(PointMutationType type, double chance, double strength = 1.0)
        {
            Contract.Requires(chance >= 0.0 && chance <= 1.0);
            Contract.Requires(strength >= 0.0 && strength <= 1.0);

            this.type = type;
            this.chance = chance;
            this.strength = strength;
        }

        private PointMutationType type;

        public PointMutationType Type
        {
            get { return type; }
            private set { type = value; }
        }

        private double chance;

        public double Chance
        {
            get { return chance; }
            private set { chance = value; }
        }

        private double strength;

        public double Strength
        {
            get { return strength; }
            private set { strength = value; }
        }
    }
}
