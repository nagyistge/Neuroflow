using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.Algorithms.Selection
{
    public sealed class BorderSelectionAlgorithm : SelectionAlgorithm
    {
        public BorderSelectionAlgorithm(SelectionDirection direction = SelectionDirection.FromTop)
        {
            Direction = direction;
        }
        
        public SelectionDirection Direction { get; private set; }

        protected override int GetNext(IntRange fromRange, int soFar)
        {
            if (Direction == SelectionDirection.FromTop)
            {
                return fromRange.MinValue + soFar;
            }
            else
            {
                return fromRange.MaxValue + soFar;
            }
        }
    }
}
