using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Neuroflow.ComputationAPI
{
    public sealed class ComputationBlock
    {
        internal const string ValueMemberName = "value";

        public ComputationBlock()
            : this(0)
        {
        }

        public ComputationBlock(int timeFrame)
        {
            Contract.Requires(timeFrame >= 0);

            this.timeFrame = timeFrame;
            References = new HashSet<Assembly>();
            Namespaces = new HashSet<string>();
            CodeBuilder = new StringBuilder();

            AddReference(typeof(int)); // System
            AddReference(typeof(Parallel));
            References.Add(GetType().Assembly);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(References != null);
            Contract.Invariant(CodeBuilder != null);
            Contract.Invariant(timeFrame >= 0);
        }

        internal HashSet<Assembly> References { get; private set; }

        internal HashSet<string> Namespaces { get; private set; }

        internal StringBuilder CodeBuilder { get; private set; }

        int timeFrame;

        public int TimeFrame
        {
            get { return timeFrame; }
            set
            {
                Contract.Requires(value >= 0);

                timeFrame = value;
            }
        }

        public bool IsEmpty
        {
            get { return CodeBuilder.Length == 0; }
        }

        public void AddReference(Type type)
        {
            Contract.Requires(type != null);

            References.Add(type.Assembly);
            Namespaces.Add(type.Namespace);
        }

        public void Add(string codeLine)
        {
            Contract.Requires(!String.IsNullOrEmpty(codeLine));

            codeLine = codeLine.Trim();

            if (!string.IsNullOrEmpty(codeLine))
            {
                if (codeLine[codeLine.Length - 1] != ';') codeLine += ';';
            }

            CodeBuilder.AppendLine(codeLine);
        }

        public void Add(ComputationBlock block)
        {
            Contract.Requires(block != null);

            foreach (var r in block.References)
            {
                References.Add(r);
            }

            foreach (var ns in block.Namespaces)
            {
                Namespaces.Add(ns);
            }

            CodeBuilder.Append(block.CodeBuilder);
        }
    }
}
