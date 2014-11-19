using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nu.Vs.Extension
{
    public class FrameworkDefinition
    {
        public string NugetAbbreviation { get; set; }
        public string TargetFramework { get; set; }

        public FrameworkDefinition(string nugetAbbr, string target)
        {
            NugetAbbreviation = nugetAbbr;
            TargetFramework = target;
        }
    }
}
