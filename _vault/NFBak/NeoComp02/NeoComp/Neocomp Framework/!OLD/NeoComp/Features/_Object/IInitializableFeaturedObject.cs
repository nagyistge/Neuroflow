using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Features
{
    public interface IInitializableFeaturedObject
    {
        void Initialize(string featureID);

        void Uninitialize();
    }
}
