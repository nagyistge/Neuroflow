using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoComp.Core;
using System.Threading;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public class GAContext
    {
        #region Constructors

        public GAContext(params IPopulation[] populations)
            : this((IEnumerable<IPopulation>)populations)
        {
        }

        public GAContext(IEnumerable<IPopulation> populations)
        {
            this.populations = populations == null ? new List<IPopulation>() : populations.ToList();
            if (this.populations.Count > 0) foreach (var population in this.populations) AddHandlers(population);
        }

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(populations != null);
        }

        #endregion

        #region Fields

        List<IPopulation> populations;

        object populationSync = new object();

        Task populationTask;

        #endregion

        #region Population List Handling

        public void Add(IPopulation population)
        {
            Contract.Requires(population != null);
            
            lock (populationSync)
            {
                populations.Add(population);
                AddHandlers(population);
            }
        }

        public void Remove(IPopulation population)
        {
            Contract.Requires(population != null);

            lock (populationSync)
            {
                int idx = populations.IndexOf(population);
                if (idx != -1)
                {
                    populations.RemoveAt(idx);
                    RemoveHandlers(population);
                }
            }
        }

        #endregion

        #region Events

        private void AddHandlers(IPopulation population)
        {
            population.BestBodyArrived += PopulationBestBodyArrived;
        }

        private void RemoveHandlers(IPopulation population)
        {
            population.BestBodyArrived -= PopulationBestBodyArrived;
        }

        void PopulationBestBodyArrived(object sender, BestBodyArrivedToGroupEventArgs e)
        {
            OnBestBodyArrived(new BestBodyArrivedToPopulationEventArgs((IPopulation)sender, e.Group, e.Body));
        }

        public event EventHandler<BestBodyArrivedToPopulationEventArgs> BestBodyArrived;

        protected virtual void OnBestBodyArrived(BestBodyArrivedToPopulationEventArgs e)
        {
            var handler = BestBodyArrived;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Population

        public void Start()
        {
            lock (populationSync)
            {
                if (populationTask == null)
                {
                    populationTask = new TaskFactory().StartNew(() =>
                    {
                        lock (populations)
                        {
                            foreach (var population in populations)
                            {
                                population.Initialize();
                                if (populationTask.IsCancellationRequested) break;
                            }
                        }

                        while (!populationTask.IsCancellationRequested)
                        {
                            IPopulation currentPopulation = null;
                            lock (populations)
                            {
                                if (populations.Count == 1)
                                {
                                    currentPopulation = populations[0];
                                }
                                else if (populations.Count > 1)
                                {
                                    currentPopulation = populations[RandomGenerator.Random.Next(populations.Count)];
                                }
                            }
                            if (currentPopulation != null) currentPopulation.Step();
                        }
                        lock (populationSync) populationTask = null;
                    });
                }
            }
        }

        public void Stop()
        {
            Task wait = null;
            if (populationTask != null)
            {
                lock (populationSync)
                {
                    if (populationTask != null)
                    {
                        wait = populationTask;
                    }
                }
            }
            if (wait != null) wait.CancelAndWait();
        }

        #endregion
    }
}
