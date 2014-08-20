using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    public sealed class EntityDataUnit
    {
        public EntityDataUnit(string optUnitID, object entityData)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(optUnitID));

            OptUnitID = optUnitID;
            EntityData = entityData;
        }
        
        public string OptUnitID { get; private set; }

        public object EntityData { get; private set; }
    }
}
