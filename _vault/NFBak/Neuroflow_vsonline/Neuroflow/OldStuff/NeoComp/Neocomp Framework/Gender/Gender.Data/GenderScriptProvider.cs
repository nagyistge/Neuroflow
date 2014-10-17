using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Learning;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace Gender.Data
{
    public sealed class GenderScriptProvider : ScriptCollectionProvider
    {
        public GenderScriptProvider(bool isTraining, int count, bool binaryOutput = true, int numberOfIterations = 1)
        {
            Contract.Requires(count > 0);
            Contract.Requires(numberOfIterations > 0);

            this.isTraining = isTraining;
            this.binaryOutput = binaryOutput;
            this.count = count;
            NumberOfIterations = numberOfIterations;
        }

        Dictionary<int, int> indexMap = new Dictionary<int, int>();

        private bool isTraining, binaryOutput;

        public int NumberOfIterations { get; private set; }

        private int count;

        public override int Count
        {
            get { return count; }
        }

        public int InputSize
        {
            get { return GenderComputation.PicWidth * GenderComputation.PicHeight; }
        }

        public int OutputSize
        {
            get { return binaryOutput ? 2 : 1; }
        }

        public override Script this[int index]
        {
            get
            {
                Contract.Requires(index >= 0 && index < Count);

                if (indexMap.Count != Count) throw new InvalidOperationException("Gender Script provider is not initialized.");

                using (var ctx = new GenderEntities())
                {
                    index = indexMap[index];
                    var item = ctx.Items.Where(i => i.ID == index).First();
                    return CreateScript(item);
                }
            }
        }

        public override IEnumerable<Script> GetScripts(ISet<int> indexes)
        {
            if (indexMap.Count != Count) throw new InvalidOperationException("Gender Script provider is not initialized.");

            int miIdx = 0;
            var mappedIndexes = new int[indexes.Count];
            foreach (int index in indexes)
            {
                mappedIndexes[miIdx++] = indexMap[index];
            }
            
            using (var ctx = new GenderEntities())
            {
                var q = ctx.Items.Where(i => mappedIndexes.Contains(i.ID));
                foreach (var item in q)
                {
                    yield return CreateScript(item);
                }
            }
        }

        public override void Reinitialize()
        {
            indexMap.Clear();
            using (var ctx = new GenderEntities())
            {
                var q = isTraining ? ctx.Items.Where(i => i.IsTrainingPattern) : ctx.Items.Where(i => !i.IsTrainingPattern);
                var dbIdxEnum = q.Select(i => i.ID).AsEnumerable().OrderByRandom().GetEnumerator();
                for (int idx = 0; idx < count && dbIdxEnum.MoveNext(); idx++)
                {
                    indexMap.Add(idx, dbIdxEnum.Current);
                }
                count = indexMap.Count;
            }

            if (isTraining) Console.WriteLine("Training reinitialized."); else Console.WriteLine("Validation reinitialized.");
        }

        private Script CreateScript(Item item)
        {
            int pixelCount = GenderComputation.PicWidth * GenderComputation.PicHeight;
            double?[] buff = new double?[pixelCount];
            for (int idx = 0; idx < pixelCount; idx++) buff[idx] = Helpers.PixelToDouble(item.Pixels[idx]);
            return new Script(new ScriptEntry(buff, GetOutput(item.Gender), NumberOfIterations));
        }

        double?[] GetOutput(bool gender)
        {
            if (binaryOutput)
            {
                double? v0 = !gender ? 1.0 : -1.0;
                double? v1 = gender ? 1.0 : -1.0;
                return new[] { v0, v1 };
            }
            else
            {
                return new double?[] { GenderToDouble(gender) };
            }
        }

        static double GenderToDouble(bool gender)
        {
            return gender ? 1.0 : -1.0;
        }

        static double Denormalize(double value)
        {
            return (value * 2.0) - 1.0;
        }
    }
}
