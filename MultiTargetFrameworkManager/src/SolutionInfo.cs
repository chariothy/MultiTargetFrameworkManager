using System.Collections.Generic;
using System.Linq;

using System.ComponentModel;

using EnvDTE;
using EnvDTE80;
using System.IO;

namespace Nu.Vs.Extension
{
    public class SolutionInfo : INotifyPropertyChanged
    {
        private EnvDTE80.DTE2 m_dte2 = null;
        private Solution2 m_solution = null;

        private string m_fullName = string.Empty;

        private string m_name = string.Empty;
        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name != value)
                {
                    m_name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        private List<SolutionConfiguration> m_solutionConfigs = new List<SolutionConfiguration>();

        public List<SolutionConfiguration> SolutionConfigs
        {
            get { return m_solutionConfigs; }
        }

        public SolutionConfiguration ActiveConfig
        {
            get { return m_solution.SolutionBuild.ActiveConfiguration; }
            set { value.Activate(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }        

        public Dictionary<string, ProjectInfo> SourceProject { get; set; }

        public SolutionInfo()
        {
            m_dte2 = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.
                    GetActiveObject("VisualStudio.DTE.12.0");
            m_solution = (Solution2)m_dte2.Solution;

        }

        public void Load()
        {
            this.m_fullName = m_solution.FullName;
            this.m_name = Path.GetFileName(this.m_fullName);
            foreach (SolutionConfiguration config in m_solution.SolutionBuild.SolutionConfigurations)
            {
                this.m_solutionConfigs.Add(config);
            }

            SourceProject = new Dictionary<string, ProjectInfo>();
            foreach (Project proj in m_solution.Projects)
            {
                bool isSourceProject = true;
                foreach (FrameworkDefinition def in MtfSetting.Instance.SupportedFrameworks)
                {
                    if (proj.Name.ToLower().EndsWith(MtfSetting.Instance.Separator + def.NugetAbbreviation))
                    {
                        isSourceProject = false;
                        break;
                    }
                }
                if (isSourceProject)
                {
                    SourceProject[proj.Name] = new ProjectInfo(proj);
                }
            }
        }

        public void ActivateConfiguration(string key)
        {
            m_solutionConfigs.First(sluc=>sluc.Name == key).Activate();
        }
    }
}
