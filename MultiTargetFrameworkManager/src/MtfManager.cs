using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE80;
using EnvDTE;

namespace Nu.Vs.Extension
{
    public class MtfManager
    {
        private SolutionInfo m_solutionInfo = new SolutionInfo();

        public SolutionInfo SolutionInfo
        {
            get { return m_solutionInfo; }
            set { m_solutionInfo = value; }
        }


        public MtfManager()
        {
            m_solutionInfo.Load();
        }
    }
}
