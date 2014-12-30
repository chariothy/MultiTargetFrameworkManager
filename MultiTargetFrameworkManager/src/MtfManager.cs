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
        private SolutionInfo m_solutionInfo = null;// new SolutionInfo();

        public SolutionInfo SolutionInfo
        {
            get { return m_solutionInfo; }
            set { m_solutionInfo = value; }
        }


        public MtfManager()
        {
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            const string directorySeparatorChar = "\\";
            Uri pathUri = new Uri(filespec);

            if (!folder.EndsWith(directorySeparatorChar))
            {
                folder += directorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace("/", directorySeparatorChar));
        }

        public static string AddDefineConstants(string definedConstants, FrameworkDefinition frameworkDefinition)
        {
            return definedConstants;
        }
    }
}
