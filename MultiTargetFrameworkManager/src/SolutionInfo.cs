using System.Collections.Generic;
using System.Linq;

using System.ComponentModel;

using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System;

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

        public static SolutionInfo Current = null;

        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);
        [DllImport("ole32.dll")]
        private static extern void GetRunningObjectTable(int reserved, out IRunningObjectTable prot);


        public SolutionInfo(Solution2 solution)
        {
            m_solution = solution;
            this.m_fullName = m_solution.FullName;
            this.m_name = Path.GetFileName(this.m_fullName);
        }

        public static List<SolutionInfo> GetOpenedSolutions()
        {
            var rotEntries = new List<string>();
            foreach (var process in System.Diagnostics.Process.GetProcesses())
            {
                if (process.ProcessName == "devenv" &&
                    process.MainModule.FileVersionInfo.FileDescription.Contains("Visual Studio"))
                {
                    rotEntries.Add(
                        string.Format("!VisualStudio.DTE.{0}.0:{1}",
                            process.MainModule.FileVersionInfo.FileMajorPart,
                            process.Id)
                        );
                }
            }

            var solutionInfos = new List<SolutionInfo>();

            IRunningObjectTable rot;
            GetRunningObjectTable(0, out rot);
            IEnumMoniker enumMoniker;
            rot.EnumRunning(out enumMoniker);
            enumMoniker.Reset();
            IntPtr fetched = IntPtr.Zero;
            IMoniker[] moniker = new IMoniker[1];
            while (enumMoniker.Next(1, moniker, fetched) == 0)
            {
                IBindCtx bindCtx;
                CreateBindCtx(0, out bindCtx);
                string displayName;
                moniker[0].GetDisplayName(bindCtx, null, out displayName);
                if (rotEntries.Contains(displayName))
                {
                    object comObject;
                    rot.GetObject(moniker[0], out comObject);
                    solutionInfos.Add(
                        new SolutionInfo(
                            (Solution2)((EnvDTE80.DTE2)comObject).Solution)
                        );
                }
            }
            return solutionInfos;
        }

        public void Initialize()
        {            
            foreach (SolutionConfiguration config in m_solution.SolutionBuild.SolutionConfigurations)
            {
                this.m_solutionConfigs.Add(config);
            }

            SourceProject = new Dictionary<string,ProjectInfo>();
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
                    SourceProject[proj.Name] = new ProjectInfo(proj, this);
                }
            }

            foreach (Project proj in m_solution.Projects)
            {
                foreach (FrameworkDefinition def in MtfSetting.Instance.SupportedFrameworks)
                {
                    var targetPostfix = MtfSetting.Instance.Separator + def.NugetAbbreviation;
                    if (proj.Name.ToLower().EndsWith(targetPostfix))
                    {
                        var sourceProjectName = proj.Name.Substring(0, proj.Name.Length - targetPostfix.Length);
                        if (SourceProject.ContainsKey(sourceProjectName))
                        {
                            SourceProject[sourceProjectName].TargetProject[proj.Name] = new ProjectInfo(proj, this);
                        }
                    }
                }
            }

            foreach (var proj in SourceProject.Values)
            {
                proj.RefreshTargetProject();
            }
        }

        public ProjectInfo GetTargetProjectInfo(ProjectInfo projectInfo, FrameworkDefinition target)
        {
            if (SourceProject.ContainsKey(projectInfo.Name))
            {
                if (!SourceProject[projectInfo.Name].TargetProject.ContainsKey(target.TargetFramework))
                {
                    var targetProject = projectInfo.AddTargetProject(target);
                    SourceProject[projectInfo.Name].TargetProject[target.TargetFramework] = targetProject;
                }
                return SourceProject[projectInfo.Name].TargetProject[target.TargetFramework];
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public void ActivateConfiguration(string key)
        {
            m_solutionConfigs.First(sluc=>sluc.Name == key).Activate();
        }
    }
}
