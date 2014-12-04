using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nu.Vs.Extension
{
    public class ProjectReferenceInfo
    {
        public string Path { get; set; }
        public bool CopyLocal { get; set; }
        public ProjectInfo SourceProject { get; set; }
    }
}
