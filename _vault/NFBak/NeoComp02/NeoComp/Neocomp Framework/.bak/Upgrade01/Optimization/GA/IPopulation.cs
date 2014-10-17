using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Collections;

namespace NeoComp.Optimization.GA
{
    [ContractClass(typeof(IPopulationContract))]
    public interface IPopulation : ISynchronized
    {
        Probability ChanceOfMigration { get; set; }

        IList Groups { get; }

        void Initialize();

        void Step();

        event EventHandler<BestBodyArrivedToGroupEventArgs> BestBodyArrived;
    }

    [ContractClassFor(typeof(IPopulation))]
    public class IPopulationContract : IPopulation
    {
        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            IPopulation p = this;
            Contract.Invariant(p.Groups != null);
        }
        
        Probability IPopulation.ChanceOfMigration
        {
            get { return default(Probability); }
            set { }
        }

        IList IPopulation.Groups
        {
            get { return null; }
        }

        void IPopulation.Initialize()
        {
        }

        void IPopulation.Step()
        {
        }

        event EventHandler<BestBodyArrivedToGroupEventArgs> IPopulation.BestBodyArrived
        {
            add { }
            remove { }
        }

        object ISynchronized.SyncRoot
        {
            get { return null; }
        }
    }
}
