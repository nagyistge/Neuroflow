using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Diagnostics;
using NeoComp.Core;
using System.Threading.Tasks;

namespace NeoComp.Optimization.Algorithms.Quantum
{
    public sealed class CompetitiveQuantumAnnealing<T> : IAnnealing
        where T : AdaptiveQuantumAnnealing
    {
        public CompetitiveQuantumAnnealing(bool parallel = true, params T[] annealingProcesses)
            : this(annealingProcesses, parallel)
        {
            Contract.Requires(annealingProcesses != null && annealingProcesses.Length > 1);
        }
        
        public CompetitiveQuantumAnnealing(IEnumerable<T> annealingProcesses, bool parallel = true)
        {
            Contract.Requires(annealingProcesses != null);

            this.processes = annealingProcesses.ToArray();
            if (this.processes.Length < 2) throw new ArgumentException("Annealing process collection must contain at least two values.", "annealingProcesses");
            this.parallel = parallel;
        }

        T[] processes;

        bool parallel;

        public T BestProcess { get; private set; }

        public T WorstProcess { get; private set; }

        public double CurrentEnergy
        {
            get { return BestProcess == null ? 1.0 : BestProcess.CurrentEnergy; }
        }

        public void Reset()
        {
            BestProcess = WorstProcess = null;
            foreach (T process in processes) process.Reset();
        }

        public void Step()
        {
            if (parallel) ParallelStep(); else SequentialStep();
        }

        private void ParallelStep()
        {
            SpinLock sl = new SpinLock();
            T best = null, worst = null;

            Parallel.For(0, processes.Length, (idx) =>
            {
                var current = processes[idx];
                current.Step();
                double energy = current.CurrentEnergy;

                bool locked = false; 
                sl.Enter(ref locked);

                if (best == null || (energy < best.CurrentEnergy)) best = current;
                if (worst == null || (energy > worst.CurrentEnergy)) worst = current;

                sl.Exit();
            });

            if (RandomGenerator.Random.NextDouble() < worst.CurrentEnergy)
            {
                worst.Blend(best);
                //Console.WriteLine(best.GetHashCode().ToString() + " - " + worst.GetHashCode());
            }

            BestProcess = best;
            WorstProcess = worst;
        }

        private void SequentialStep()
        {
            T best = null, worst = null;

            for (int idx = 0; idx < processes.Length; idx++)
            {
                var current = processes[idx];
                current.Step();
                double energy = current.CurrentEnergy;

                if (best == null || (energy < best.CurrentEnergy)) best = current;
                if (worst == null || (energy > worst.CurrentEnergy)) worst = current;
            }

            if (RandomGenerator.Random.NextDouble() < worst.CurrentEnergy)
            {
                worst.Blend(best);
                //Console.WriteLine(best.GetHashCode().ToString() + " - " + worst.GetHashCode());
            }

            BestProcess = best;
            WorstProcess = worst;
        }
    }
}
