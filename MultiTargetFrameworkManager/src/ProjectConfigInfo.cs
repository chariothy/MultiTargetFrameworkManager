using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nu.Vs.Extension
{
    public class ProjectConfigInfo
    {
        public string Name { get; set; }
        public string OutputPath { get; set; }
        public string DefineConstants { get; set; }
        public string PlatformTarget { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
