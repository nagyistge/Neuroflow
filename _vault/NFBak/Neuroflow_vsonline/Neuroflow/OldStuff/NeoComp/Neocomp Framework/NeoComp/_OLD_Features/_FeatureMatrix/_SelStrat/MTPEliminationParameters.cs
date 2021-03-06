﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    public sealed class MTPEliminationParameters
    {
        public MTPEliminationParameters(int minIterations = 1, int maxIterations = 5)
        {
            Contract.Requires(minIterations >= 1);
            Contract.Requires(maxIterations > minIterations);

            MinIterations = minIterations;
            MaxIterations = maxIterations;
        }
        
        public int MinIterations { get; private set; }

        public int MaxIterations { get; private set; }
    }
}
