using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Features
{
    public enum MonteCarloMode : byte { ReplaceOneItem, NewContinuousBlock, NewBlock }

    public sealed class MonteCarloDataFeatureSelectionStrategy : DataFeatureSelectionStrategy
    {
        #region Constructor

        public MonteCarloDataFeatureSelectionStrategy(int groupSize, MonteCarloMode mode)
        {
            Contract.Requires(groupSize > 0);

            Mode = mode;
            GroupSize = groupSize;
        }

        #endregion

        #region Properties And Fields

        int count;

        Dictionary<int, int> currentBlock;

        public MonteCarloMode Mode { get; private set; }

        public int GroupSize { get; private set; }

        #endregion

        #region Init

        protected override void Initialize()
        {
            currentBlock = null;
            count = Owner.DataFeatureProvider.ItemCount;
            if (GroupSize > count) GroupSize = count;
        }

        protected internal override void Uninitialize()
        {
            currentBlock = null;
        }

        #endregion

        #region Select

        protected internal override FeatureIndexSet? GetNextIndexes()
        {
            switch (Mode)
            {
                case MonteCarloMode.ReplaceOneItem:
                    return ReplaceOneItemSelect();
                case MonteCarloMode.NewContinuousBlock:
                    return NewContinuousBlockSelect();
                default:
                    return NewBlockSelect();
            }
        }

        private FeatureIndexSet ReplaceOneItemSelect()
        {
            int idx = RandomGenerator.Random.Next(count);

            if (currentBlock == null)
            {
                currentBlock = new Dictionary<int, int>();

                while (currentBlock.Count != GroupSize)
                {
                    while (currentBlock.ContainsKey(idx)) idx = RandomGenerator.Random.Next(count);
                    currentBlock.Add(idx, idx);
                }
            }
            else
            {
                int removeIdx = currentBlock.Keys.OrderByRandom().First();
                currentBlock.Remove(removeIdx);
                while (idx == removeIdx || currentBlock.ContainsKey(idx)) idx = RandomGenerator.Random.Next(count);
                currentBlock.Add(idx, idx);
            }

            return new FeatureIndexSet(currentBlock.Values, true);
        }

        private FeatureIndexSet NewContinuousBlockSelect()
        {
            int startIndex = RandomGenerator.Random.Next(count - GroupSize);
            return new FeatureIndexSet(startIndex, GroupSize, true);
        }

        private FeatureIndexSet NewBlockSelect()
        {
            var indexes = new HashSet<int>();
            int idx = RandomGenerator.Random.Next(count);

            while (indexes.Count != GroupSize)
            {
                while (indexes.Contains(idx)) idx = RandomGenerator.Random.Next(count);
                indexes.Add(idx);
            }

            return new FeatureIndexSet(indexes);
        }

        #endregion
    }
}
