using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nu.Vs.Extension
{
    class SolutionConfig
    {
        private string m_activeConfig;

        public string ActiveConfig
        {
            get { return m_activeConfig; }
            set { m_activeConfig = value; }
        }

        public List<string> SolutionConfigs { get; set; }
    }
}
