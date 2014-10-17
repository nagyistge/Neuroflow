using Neuroflow.Core;
using Neuroflow.Core.Algorithms.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public sealed class TournamentSelectionAlgorithm : ISelectionAlgorithm
    {
        public TournamentSelectionAlgorithm(int tournamentSize = 5)
        {
            Contract.Requires(tournamentSize > 0);

            TournamentSize = tournamentSize;
        }

        public int TournamentSize { get; private set; }
        
        public ISet<int> Select(IntRange fromRange, int count)
        {
            int size = TournamentSize;
            if (size < count) size = count;

            var arena = new SortedSet<int>();
            while (arena.Count != size)
            {
                arena.Add(fromRange.PickRandomValue());
            }

            return new HashSet<int>(arena.Take(count));
        }
    }
}
