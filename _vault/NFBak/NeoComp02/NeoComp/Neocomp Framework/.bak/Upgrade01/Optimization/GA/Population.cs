using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Collections;

namespace NeoComp.Optimization.GA
{
    public class Population<TBodyPlan, TBody> : SynchronizedObject, IPopulation
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        #region Contructor

        public Population()
        {
            Groups = new GroupCollection<TBodyPlan, TBody>(this);
        }

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(Groups != null);
        }

        #endregion

        #region Properties

        public GroupCollection<TBodyPlan, TBody> Groups { get; private set; }

        Probability chanceOfMigration = 0.1;

        public Probability ChanceOfMigration
        {
            get { lock (SyncRoot) return chanceOfMigration; }
            set { lock (SyncRoot) chanceOfMigration = value; }
        }

        #endregion

        #region Groups

        internal void GroupAdded(Group<TBodyPlan, TBody> group)
        {
            Contract.Requires(group != null);

            group.BestBodyArrived += GroupOnBestBodyArrived;
        }

        internal void GroupRemoved(Group<TBodyPlan, TBody> group)
        {
            Contract.Requires(group != null);

            group.BestBodyArrived -= GroupOnBestBodyArrived;
        }

        #endregion

        #region Population

        public void Initialize()
        {
            lock (SyncRoot)
            {
                var groups = Groups.InternalList;
                var parentTask = Task.Current;
                var cToken = parentTask == null ? default(CancellationToken?) : parentTask.CancellationToken;
                foreach (var group in groups)
                {
                    group.GAStarted(cToken);
                    if (parentTask != null && parentTask.IsCancellationRequested) break;
                }
            }
        }

        public void Step()
        {
            lock (SyncRoot)
            {
                var groups = Groups.InternalList; 
                if (groups.Count > 0)
                {
                    var parentTask = Task.Current;
                    var cToken = parentTask == null ? default(CancellationToken?) : parentTask.CancellationToken;
                    var combinedBodies = new TBody[groups.Count];
                    var options = parentTask != null ?
                        new ParallelOptions { CancellationToken = parentTask.CancellationToken } :
                        new ParallelOptions();

                    Parallel.For(0, groups.Count, options, idx =>
                    {
                        var group = groups[idx];
                        Contract.Assume(group != null);
                        combinedBodies[idx] = group.CreateOffspring(cToken);
                    });

                    if (parentTask == null || !parentTask.IsCancellationRequested)
                    {
                        var migChance = ChanceOfMigration;
                        Parallel.For(0, groups.Count, options, idx =>
                        {
                            var combinedBody = combinedBodies[idx];
                            if (combinedBody != null)
                            {
                                if (migChance)
                                {
                                    GetNearestGroup(idx).Add(combinedBody);
                                }
                                else
                                {
                                    var group = groups[idx];
                                    Contract.Assume(group != null);
                                    group.Add(combinedBody);
                                }
                            }
                        });
                    }
                }
            }
        }

        private Group<TBodyPlan, TBody> GetNearestGroup(int groupIdx)
        {
            Contract.Ensures(Contract.Result<Group<TBodyPlan, TBody>>() != null);

            var groups = Groups.InternalList;
            int nIdx;

            if (groups.Count == 1)
            {
                nIdx = 0;
            }
            else if (groups.Count == 2)
            {
                nIdx = groupIdx == 0 ? 1 : 0;
            }
            else if (groupIdx == 0)
            {
                nIdx = RandomGenerator.FiftyPercentChance ? 1 : groups.Count - 1;
            }
            else if (groupIdx == groups.Count - 1)
            {
                nIdx = RandomGenerator.FiftyPercentChance ? 0 : groups.Count - 2;
            }
            else
            {
                nIdx = RandomGenerator.FiftyPercentChance ? groupIdx - 1 : groupIdx + 1;
            }

            Contract.Assume(nIdx >= 0 && nIdx < groups.Count);

            var group = groups[nIdx];
            Contract.Assume(group != null);

            return group;
        }

        #endregion

        #region Events

        void GroupOnBestBodyArrived(object sender, BestBodyArrivedEventArgs<TBodyPlan, TBody> e)
        {
            Contract.Requires(e != null);
            Contract.Requires(e.Body != null);
            Contract.Requires(sender != null);
            Contract.Requires(sender is Group<TBodyPlan, TBody>);

            OnBestBodyArrived(new BestBodyArrivedToGroupEventArgs<TBodyPlan, TBody>(e.Body, (Group<TBodyPlan, TBody>)sender));
        }

        public event EventHandler<BestBodyArrivedToGroupEventArgs<TBodyPlan, TBody>> BestBodyArrived;

        protected virtual void OnBestBodyArrived(BestBodyArrivedToGroupEventArgs<TBodyPlan, TBody> e)
        {
            var handler = BestBodyArrived;
            if (handler != null) handler(this, e);
            var helper = BestBodyArrivedHelper;
            if (helper != null) helper(this, e);
        }

        #endregion

        #region IPopulation Members

        IList IPopulation.Groups
        {
            get { return Groups; }
        }

        void IPopulation.Initialize()
        {
            Initialize();
        }

        void IPopulation.Step()
        {
            Step();
        }

        private event EventHandler<BestBodyArrivedToGroupEventArgs> BestBodyArrivedHelper;

        event EventHandler<BestBodyArrivedToGroupEventArgs> IPopulation.BestBodyArrived
        {
            add { BestBodyArrivedHelper += value; }
            remove { BestBodyArrivedHelper -= value; }
        }

        #endregion
    }
}
