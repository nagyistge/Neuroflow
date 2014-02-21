using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class LearningLayerGroups : ReadOnlyCollection<LearningLayerGroup>
    {
        internal LearningLayerGroups(ConnectedLayerGroups groups)
            : base(CreateItems(groups))
        {
        }

        private static LearningLayerGroup[] CreateItems(ConnectedLayerGroups groups)
        {
            Contract.Requires(groups != null);

            var q = from cl in groups.Groups.SelectMany(g => g)
                    let hlr = cl.Layer as IHasLearningRules
                    where hlr != null
                    from r in hlr.LearningRules
                    let type = r.GetType()
                    let gid = r.GroupID
                    group cl by r into g
                    select new LearningLayerGroup
                    {
                        Rule = g.Key,
                        ConnectedLayers = Array.AsReadOnly(g.ToArray())
                    };

            return q.ToArray();
        }
    }
}
