using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;
using System.Diagnostics.Contracts;

namespace NeoComp.Activities.Design.ViewModel
{
    public sealed class InArgumentExpression
    {
        public InArgumentExpression(string name, ModelItem ownerModelItem, ModelItem expressionModelItem)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(ownerModelItem != null);
            Contract.Requires(expressionModelItem != null);

            Name = name;
            OwnerModelItem = ownerModelItem;
            ExpressionModelItem = expressionModelItem;
        }

        public string Name { get; private set; }

        public Type Type { get; private set; }

        public ModelItem OwnerModelItem { get; private set; }

        public ModelItem ExpressionModelItem { get; private set; }
    }
}
