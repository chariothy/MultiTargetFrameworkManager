using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Web.Script.Serialization;

using EnvDTE;
using EnvDTE80;
using System.IO;

namespace Nu.Vs.Extension
{
    public class SolutionInfo : INotifyPropertyChanged
    {
        private static string s_frameworkPath = "Frameworks.json";

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

        public List<FrameworkDefinition> SupportedFrameworks { get; set; }

        public SolutionInfo()
        {
            m_dte2 = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.
                    GetActiveObject("VisualStudio.DTE.12.0");
            m_solution = (Solution2)m_dte2.Solution;

            SupportedFrameworks =
                new List<FrameworkDefinition>
                {
                      new FrameworkDefinition("net452", ".NETFramework,Version=v4.5.2"),
                      new FrameworkDefinition("net451", ".NETFramework,Version=v4.5.1"),
                      new FrameworkDefinition("net45", ".NETFramework,Version=v4.5"),
                      new FrameworkDefinition("net40", ".NETFramework,Version=v4.0"),
                      new FrameworkDefinition("net40-client", ".NETFramework,Version=v4.0,Profile=Client"),
                      new FrameworkDefinition("net35", ".NETFramework,Version=v3.5"),
                      new FrameworkDefinition("net35-client", ".NETFramework,Version=v3.5,Profile=Client"),
                      new FrameworkDefinition("net30", ".NETFramework,Version=v3.0"),
                      new FrameworkDefinition("net20", ".NETFramework,Version=v2.0")
                };
        }

        public void Load()
        {
            var serializer = new JavaScriptSerializer();
            if(File.Exists(s_frameworkPath)) {
                using (var stream = new StreamReader(s_frameworkPath))
                {
                    SupportedFrameworks = serializer.Deserialize<List<FrameworkDefinition>>(stream.ReadToEnd());
                }
            }
            else
            {
                using (var stream = new StreamWriter(s_frameworkPath))
                {
                    var json = serializer.Serialize(SupportedFrameworks);
                    var jsonPretty = JSON_PrettyPrinter.Process(json);
                    stream.Write(jsonPretty);
                }
            }

            this.m_fullName = m_solution.FullName;
            this.m_name = Path.GetFileName(this.m_fullName);
            foreach (SolutionConfiguration config in m_solution.SolutionBuild.SolutionConfigurations)
            {
                this.m_solutionConfigs.Add(config);
            }
        }

        public void ActivateConfiguration(string key)
        {
            m_solutionConfigs.First(sluc=>sluc.Name == key).Activate();
        }
    }
}
