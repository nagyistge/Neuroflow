using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public struct Device
    {
        public Device(string id, string name, string platform)
        {
            Args.Requires(() => id, () => !string.IsNullOrWhiteSpace(id));

            this.id = id;
            this.name = name;
            this.platform = platform;
        }

        string platform, id, name;

        public string Platform
        {
            get { return platform; }
        }

        public string ID
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public override string ToString()
        {
            return id;
        }
    }
}
