using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Core;

namespace ColorTest
{
    public sealed class Genom : IEquatable<Genom>, IComparable<Genom>
    {
        public Genom(double[] genes)
        {
            Contract.Requires(genes != null);
            Contract.Requires(genes.Length > 0);

            this.uid = Guid.NewGuid();
            this.genes = genes;
        }

        public static Genom CreateRandom(int length)
        {
            Contract.Requires(length > 0);

            return new Genom(CreateRandomGenes(length));
        }

        public static double[] CreateRandomGenes(int length)
        {
            Contract.Requires(length > 0);

            var genes = new double[length];
            for (int i = 0; i < genes.Length; i++) genes[i] = RandomGenerator.Random.NextDouble();
            return genes;
        }

        private Guid uid;

        object data;

        public object Data
        {
            get { return data; }
            set { data = value; }
        }

        private double[] genes;

        public double[] Genes
        {
            get { return genes; }
            private set { genes = value; }
        }

        public bool Equals(Genom other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            return uid == other.uid;
        }

        public override bool Equals(object obj)
        {
            return obj is Genom ? Equals((Genom)obj) : false;
        }

        public override int GetHashCode()
        {
            return uid.GetHashCode();
        }

        public override string ToString()
        {
            return string.Join(" ", genes.Select(g => g.ToString("0.0000")));
        }

        public static bool operator ==(Genom g1, Genom g2)
        {
            if (object.ReferenceEquals(g1, null)) return object.ReferenceEquals(g2, null);
            return g1.Equals(g2);
        }

        public static bool operator !=(Genom g1, Genom g2)
        {
            return !g1.Equals(g2);
        }

        int IComparable<Genom>.CompareTo(Genom other)
        {
            if (object.ReferenceEquals(other, null)) throw new ArgumentNullException("other");
            return uid.CompareTo(other.uid);
        }
    }
}
