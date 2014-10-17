using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Evolution.Statistical
{
    public class SetOptUnit<T> : DiscreteOptUnit<T>
    {
        #region Construct

        public SetOptUnit(string id, int resolution, ISet<T> items)
            : base(id, resolution)
        {
            Contract.Requires(!string.IsNullOrEmpty(id)); 
            Contract.Requires(resolution > 1);
            Contract.Requires(items != null);
            Contract.Requires(items.Count != 0);

            Items = items;
            ItemArray = items.ToArray();
        }

        #endregion

        #region Properties

        public ISet<T> Items { get; private set; }

        protected T[] ItemArray { get; private set; }

        #endregion

        #region Impl

        protected override T CreateRandomValue()
        {
            return ItemArray[RandomGenerator.Random.Next(ItemArray.Length)];
        }

        protected override T CreateRandomValueExcept(HashSet<T> items)
        {
            var rnd = ItemArray[RandomGenerator.Random.Next(ItemArray.Length)];
            while (items.Contains(rnd))
            {
                rnd = ItemArray[RandomGenerator.Random.Next(ItemArray.Length)];
            }
            return rnd;
        } 

        #endregion
    }
}
