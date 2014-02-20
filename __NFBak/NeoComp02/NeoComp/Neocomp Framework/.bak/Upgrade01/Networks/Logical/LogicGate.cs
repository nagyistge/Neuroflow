using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Core;
using System.Collections.ObjectModel;

namespace NeoComp.Networks.Logical
{
    [Flags]
    public enum LogicGateType
    {
        AND = 1,
        OR = 2,
        NAND = 4,
        NOR = 8,
        XOR = 16,
        XNOR = 32
    }

    public sealed class LogicGate : ComputationalNode<LogicalConnection, bool>
    {
        #region Constructor

        public LogicGate(LogicGateType type) : this(type, null) { }

        public LogicGate(LogicGateType type, LogicGateInputRestrictions restrictions)
        {
            if ((type & LogicGateType.AND) == LogicGateType.AND)
            {
                Type = LogicGateType.AND;
                getOutputMethod = GetAND;
            }
            else if ((type & LogicGateType.OR) == LogicGateType.OR)
            {
                Type = LogicGateType.OR;
                getOutputMethod = GetOR;
            }
            else if ((type & LogicGateType.NAND) == LogicGateType.NAND)
            {
                Type = LogicGateType.NAND;
                getOutputMethod = GetNAND;
            }
            else if ((type & LogicGateType.NOR) == LogicGateType.NOR)
            {
                Type = LogicGateType.NOR;
                getOutputMethod = GetNOR;
            }
            else if ((type & LogicGateType.XOR) == LogicGateType.XOR)
            {
                Type = LogicGateType.XOR;
                getOutputMethod = GetXOR;
            }
            else // if ((type & LogicGateType.XNOR) == LogicGateType.XNOR)
            {
                Type = LogicGateType.XNOR;
                getOutputMethod = GetXNOR;
            }
            if (restrictions != null) restriction = restrictions[Type];
        }

        #endregion

        #region Fields

        Func<LogicalConnection[], bool> getOutputMethod;

        IntRange? restriction;

        #endregion

        #region Properties

        public LogicGateType Type { get; private set; }

        #endregion

        #region Output

        protected override bool GenerateOutput(LogicalConnection[] inputConnections, LogicalConnection[] outputConnections, out bool output)
        {
            if (inputConnections.Length > 0 &&
                (!restriction.HasValue ||
                (restriction.HasValue && restriction.Value.IsIn(inputConnections.Length))))
            {
                output = getOutputMethod(inputConnections);
                return true;
            }
            else
            {
                return output = false;
            }
        }

        #endregion

        #region AND

        private bool GetAND(LogicalConnection[] inputConnections)
        {
            int count = 0;
            foreach (var conn in inputConnections)
            {
                if (conn.BitValue) count++;
            }
            return count == inputConnections.Length;
        }

        #endregion

        #region NAND

        private bool GetNAND(LogicalConnection[] inputConnections)
        {
            return !GetAND(inputConnections);
        }

        #endregion

        #region OR

        private bool GetOR(LogicalConnection[] inputConnections)
        {
            return !GetNOR(inputConnections);
        }

        #endregion

        #region NOR

        private bool GetNOR(LogicalConnection[] inputConnections)
        {
            foreach (var conn in inputConnections)
            {
                if (conn.BitValue) return false;
            }
            return true;
        }

        #endregion

        #region XOR

        private bool GetXOR(LogicalConnection[] inputConnections)
        {
            bool output = inputConnections[0].BitValue;
            for (int idx = 1; idx < inputConnections.Length; idx++)
            {
                output = output ^ inputConnections[idx].BitValue;
            }
            return output;
        }

        #endregion

        #region XNOR

        private bool GetXNOR(LogicalConnection[] inputConnections)
        {
            return !GetXOR(inputConnections);
        }

        #endregion
    }
}
