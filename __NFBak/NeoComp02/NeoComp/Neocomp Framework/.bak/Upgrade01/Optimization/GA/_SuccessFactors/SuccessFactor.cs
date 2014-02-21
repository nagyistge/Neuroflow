using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;

namespace NeoComp.Optimization.GA
{
    public interface ISuccessFactor : IComparable
    {
        bool IsDirty { get; }

        object Container { get; }

        ComparationMode ComparationMode { get; }

        void Update();

        event EventHandler Dirtied;
    }

    public abstract class SuccessFactor<TContainer> : ISuccessFactor, IComparable<SuccessFactor<TContainer>>
        where TContainer : class
    {
        #region Constructor

        protected SuccessFactor(TContainer container, ComparationMode comparationMode)
        {
            Args.IsNotNull(container, "container");
            Container = container;
            ComparationMode = comparationMode;
        } 

        #endregion

        #region Properties

        public bool IsDirty { get; private set; }

        public TContainer Container { get; private set; }

        public ComparationMode ComparationMode { get; private set; } 

        #endregion

        #region Update

        public abstract void Update();

        #endregion

        #region Dirty

        public event EventHandler Dirtied;

        protected virtual void OnDirtied(EventArgs e)
        {
            var handler = Dirtied;
            if (handler != null) handler(this, e);
        }

        protected void ToDirty()
        {
            if (!IsDirty)
            {
                IsDirty = true;
                OnDirtied(EventArgs.Empty);
            }
        }

        #endregion

        #region ISuccessFactor Members

        object ISuccessFactor.Container
        {
            get { return Container; }
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return DoCompareTo(Args.CastAs<SuccessFactor<TContainer>>(obj, "obj"));
        }

        public int CompareTo(SuccessFactor<TContainer> other)
        {
            Args.IsNotNull(other, "other");
            return DoCompareTo((SuccessFactor<TContainer>)other);
        }

        protected abstract int DoCompareTo(SuccessFactor<TContainer> other);

        #endregion
    }
}
