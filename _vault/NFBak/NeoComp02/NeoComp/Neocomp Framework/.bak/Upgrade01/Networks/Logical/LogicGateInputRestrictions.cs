using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;

namespace NeoComp.Networks.Logical
{
    public sealed class LogicGateInputRestrictions
    {
        static LogicGateInputRestrictions()
        {
            var res = new LogicGateInputRestrictions();
            res[LogicGateType.AND] = IntRange.CreateFixed(2);
            res[LogicGateType.NAND] = IntRange.CreateFixed(2);
            res[LogicGateType.OR] = IntRange.CreateFixed(2);
            res[LogicGateType.NOR] = IntRange.CreateFixed(2);
            res[LogicGateType.XOR] = IntRange.CreateFixed(2);
            res[LogicGateType.XNOR] = IntRange.CreateFixed(2);
            res.isSealed = true;
            Standard = res;
        }

        public static readonly LogicGateInputRestrictions Standard;

        Dictionary<LogicGateType, IntRange> restrictions = new Dictionary<LogicGateType, IntRange>();

        bool isSealed;

        public IntRange? this[LogicGateType gateType]
        {
            get
            {
                IntRange r;
                if (restrictions.TryGetValue(gateType, out r)) return r;
                return null;
            }
            set
            {
                if (isSealed) throw new InvalidOperationException("Cannot modify sealed restrictions.");
                if (value.HasValue)
                {
                    restrictions[gateType] = value.Value;
                }
                else
                {
                    restrictions.Remove(gateType);
                }
            }
        }
    }
}
