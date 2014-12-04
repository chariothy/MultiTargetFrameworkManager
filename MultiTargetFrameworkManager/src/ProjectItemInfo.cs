using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nu.Vs.Extension
{
    public class ProjectItemInfo
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsLink { get; set; }
        public bool IsDependentFile { get; set; }
    }
}
