using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Computational.Logical
{
    public sealed class LogicGate : OperationNode<bool>
    {
        #region Constructor

        public LogicGate(LogicalOperation operation)
        {
            switch (Operation = operation)
            {
                case LogicalOperation.AND:
                    getOutputMethod = GetAND;
                    break;
                case LogicalOperation.NAND:
                    getOutputMethod = GetNAND;
                    break;
                case LogicalOperation.NOR:
                    getOutputMethod = GetNOR;
                    break;
                case LogicalOperation.OR:
                    getOutputMethod = GetOR;
                    break;
                case LogicalOperation.XNOR:
                    getOutputMethod = GetXNOR;
                    break;
                default:
                    getOutputMethod = GetXOR;
                    break;
            }
        }

        #endregion

        #region Fields

        Func<ConnectionEntry<ComputationalConnection<bool>>[], bool> getOutputMethod;

        #endregion

        #region Properties

        public LogicalOperation Operation { get; private set; }

        #endregion

        #region Output

        protected override bool GenerateOutput(ConnectionEntry<ComputationalConnection<bool>>[] inputConnectionEntries)
        {
            return inputConnectionEntries.Length == 0 ? false : getOutputMethod(inputConnectionEntries);
        }

        #endregion

        #region AND

        private bool GetAND(ConnectionEntry<ComputationalConnection<bool>>[] inputConnectionEntries)
        {
            int count = 0;
            foreach (var e in inputConnectionEntries)
            {
                if (e.Connection.OutputValue) count++;
            }
            return count == inputConnectionEntries.Length;
        }

        #endregion

        #region NAND

        private bool GetNAND(ConnectionEntry<ComputationalConnection<bool>>[] inputConnectionEntries)
        {
            return !GetAND(inputConnectionEntries);
        }

        #endregion

        #region OR

        private bool GetOR(ConnectionEntry<ComputationalConnection<bool>>[] inputConnectionEntries)
        {
            return !GetNOR(inputConnectionEntries);
        }

        #endregion

        #region NOR

        private bool GetNOR(ConnectionEntry<ComputationalConnection<bool>>[] inputConnectionEntries)
        {
            foreach (var e in inputConnectionEntries)
            {
                if (e.Connection.OutputValue) return false;
            }
            return true;
        }

        #endregion

        #region XOR

        private bool GetXOR(ConnectionEntry<ComputationalConnection<bool>>[] inputConnectionEntries)
        {
            bool output = inputConnectionEntries[0].Connection.OutputValue;
            for (int idx = 1; idx < inputConnectionEntries.Length; idx++)
            {
                output = output ^ inputConnectionEntries[idx].Connection.OutputValue;
            }
            return output;
        }

        #endregion

        #region XNOR

        private bool GetXNOR(ConnectionEntry<ComputationalConnection<bool>>[] inputConnectionEntries)
        {
            return !GetXOR(inputConnectionEntries);
        }

        #endregion
    }

}
