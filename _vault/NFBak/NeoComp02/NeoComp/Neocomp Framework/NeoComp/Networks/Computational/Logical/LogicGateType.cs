using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Logical
{
    public struct LogicGateType : IEquatable<LogicGateType>
    {
        #region Construct and Create

        public LogicGateType(LogicalOperation operation, int numberOfInputs = 2)
        {
            Contract.Requires(numberOfInputs > 0);

            if (numberOfInputs == 1)
            {
                if (operation != LogicalOperation.NAND && operation != LogicalOperation.NOR) throw new ArgumentException("operation", "Only NAND or NOR gates can have one input connection.");
            }

            this.operation = operation;
            this.numberOfInputs = numberOfInputs;
        }

        public static LogicGateType NOT(LogicalOperation operation = LogicalOperation.NAND)
        {
            Contract.Requires(operation == LogicalOperation.NAND || operation == LogicalOperation.NOR);

            return operation == LogicalOperation.NAND ? NAND(1) : NOR(1);
        }

        public static LogicGateType AND(int numberOfInputs = 2)
        {
            Contract.Requires(numberOfInputs > 1);

            return new LogicGateType(LogicalOperation.AND, numberOfInputs);
        }

        public static LogicGateType OR(int numberOfInputs = 2)
        {
            Contract.Requires(numberOfInputs > 1);

            return new LogicGateType(LogicalOperation.OR, numberOfInputs);
        }

        public static LogicGateType NAND(int numberOfInputs = 2)
        {
            Contract.Requires(numberOfInputs > 0);

            return new LogicGateType(LogicalOperation.NAND, numberOfInputs);
        }

        public static LogicGateType NOR(int numberOfInputs = 2)
        {
            Contract.Requires(numberOfInputs > 0);

            return new LogicGateType(LogicalOperation.NOR, numberOfInputs);
        }

        public static LogicGateType XOR(int numberOfInputs = 2)
        {
            Contract.Requires(numberOfInputs > 1);

            return new LogicGateType(LogicalOperation.XOR, numberOfInputs);
        }

        public static LogicGateType XNOR(int numberOfInputs = 2)
        {
            Contract.Requires(numberOfInputs > 1);

            return new LogicGateType(LogicalOperation.XNOR, numberOfInputs);
        }

        #endregion

        #region Props

        LogicalOperation operation;

        public LogicalOperation Operation
        {
            get { return operation; }
        }

        int numberOfInputs;

        public int NumberOfInputs
        {
            get { return numberOfInputs; }
        }

        public bool IsValid
        {
            [Pure]
            get { return numberOfInputs != 0; }
        }

        #endregion

        #region Equality

        public bool Equals(LogicGateType other)
        {
            return operation == other.operation && numberOfInputs == other.numberOfInputs;
        }

        public static bool operator ==(LogicGateType t1, LogicGateType t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(LogicGateType t1, LogicGateType t2)
        {
            return !t1.Equals(t2);
        }

        #endregion

        #region Object

        public override bool Equals(object obj)
        {
            return obj is LogicGateType && ((LogicGateType)obj).Equals(this);
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", Operation, NumberOfInputs);
        }

        public override int GetHashCode()
        {
            return (Operation.GetHashCode() << 24) | NumberOfInputs;
        }

        #endregion
    }
}
