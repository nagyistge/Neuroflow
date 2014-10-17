using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Core
{
    public struct Probability
    {
        #region Constructors

        public Probability(double chance)
        {
            Verify(chance, "chance");
            this.chance = chance;
        }

        public Probability(Probability probability)
        {
            this.chance = probability.chance;
        } 

        #endregion

        #region Properties

        double chance;

        public double Chance
        {
            get { return chance; }
            set
            {
                Verify(value, "value");
                chance = value;
            }
        }

        public bool IsChance
        {
            get { return chance != 0.0; }
        } 

        #endregion

        #region Operators

        public static implicit operator double(Probability probability)
        {
            return probability.chance;
        }

        public static implicit operator Probability(double chance)
        {
            return new Probability(chance);
        }

        public static implicit operator bool(Probability probability)
        {
            double chance = probability.chance;
            if (chance == 0.0) return false;
            if (chance == 1.0) return true;
            return RandomGenerator.Random.NextDouble() < chance;
        } 

        #endregion

        #region Object

        public override string ToString()
        {
            return Math.Round(chance * 100.0, 4).ToString() + "%";
        }

        #endregion

        #region Verify

        private static void Verify(double value, string argName)
        {
            if (value < 0.0 || value > 1.0) throw new ArgumentOutOfRangeException(argName);
        } 

        #endregion
    }
}
