using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public sealed class TournamentSelectionStrategy : SynchronizedObject, ISelectionStrategy
    {
        #region Constructors

        public TournamentSelectionStrategy()
            : this(5, 0.75)
        {
        }

        public TournamentSelectionStrategy(int tournamentSize, Probability bestWinChance)
        {
            Contract.Requires(tournamentSize > 0);

            this.tournamentSize = tournamentSize;
            this.bestWinChance = bestWinChance;
        } 

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(tournamentSize > 0);
        }

        #endregion

        #region Properties

        int tournamentSize;

        public int TournamentSize
        {
            get { lock (SyncRoot) return tournamentSize; }
            set
            {
                Contract.Requires(value > 0);

                lock (SyncRoot) tournamentSize = value;
            }
        }

        Probability bestWinChance;

        public Probability BestWinChance
        {
            get { lock (SyncRoot) return bestWinChance; }
            set { lock (SyncRoot) bestWinChance = value; }
        } 

        #endregion

        #region Select

        public System.Collections.IEnumerable Select(ISelectableItemCollection orderedItems, int count)
        {
            int itemCount = orderedItems.Count;
            int tournamentSize = TournamentSize;
            if (tournamentSize < count) tournamentSize = count;
            if (tournamentSize > itemCount) tournamentSize = itemCount;


            var arena = new SortedDictionary<int, object>();
            while (arena.Count != tournamentSize)
            {
                int index = RandomGenerator.Random.Next(itemCount);
                if (!arena.ContainsKey(index)) arena.Add(index, orderedItems.Select(index));
            }

            var baseProbability = BestWinChance;
            var p = baseProbability;
            int returnedCount = 0;
            while (returnedCount != count)
            {
                int index = 0;
                int winnerIndex = -1;
                object winner = null;
                foreach (var kvp in arena)
                {
                    if (p)
                    {
                        winnerIndex = kvp.Key;
                        winner = kvp.Value;
                        break;
                    }
                    p = (double)baseProbability * Math.Pow((1.0 - (double)baseProbability), index++);
                }
                if (winnerIndex != -1)
                {
                    yield return winner;
                    arena.Remove(winnerIndex);
                    returnedCount++;
                }
            }
        } 

        #endregion
    }
}
