using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using EnvDTE;
using EnvDTE80;
using VSLangProj;
using System.IO;

namespace Nu.Vs.Extension
{
    public class ProjectInfo : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Dictionary<string, ProjectConfigInfo> ConfigInfo { get; set; }        
        public List<string> SupportedPlatforms { get; set; }
        public List<ProjectReferenceInfo> ReferenceInfos { get; set; }
        public List<ProjectItemInfo> ItemInfos { get; set; }
        /// <summary>
        /// Projects which have a definite target framework
        /// </summary>
        public Dictionary<FrameworkDefinition, ProjectInfo> TargetProject { get; set; }
        public string TargetFrameworkMoniker { get; set; }
        public string OutputFileName { get; set; }
        public string AssemblyVersion { get; set; }
        public string Description { get; set; }
        public string TargetFramework { get; set; }
        public FrameworkDefinition FrameworkDefinition { get; set; }
        public string FullName { get; set; }
        public string FullPath { get; set; }
        public string Company { get; set; }
        public string AssemblyFileVersion { get; set; }
        public string LocalPath { get; set; }
        public bool IsSourceProject { get; set; }
        public bool IsAddedToSolution { get; set; }

        private Project m_project = null;
        private Solution2 m_solution = null;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RemoveTargetProject(FrameworkDefinition frameworkDefinition)
        {
            m_solution.Remove(TargetProject[frameworkDefinition].m_project);
        }

        public void AddTargetProject(FrameworkDefinition frameworkDefinition)
        {
            var parentOfProjectDir = Directory.GetParent(FullName).FullName;
            var targetProjectName = Name + MtfSetting.Instance.Separator + frameworkDefinition.NugetAbbreviation;
            var targetProjectDir = Path.Combine(parentOfProjectDir, targetProjectName);
            var targetProjectPath = Path.Combine(targetProjectDir, targetProjectName);

            if (!Directory.Exists(targetProjectDir))
            {
                Directory.CreateDirectory(targetProjectDir);
            }

            if(!File.Exists(targetProjectPath))
            {
                File.Copy(FullName, targetProjectPath);
            }

            Project targetProject = m_solution.AddFromFile(targetProjectPath);
            //handle project property
            targetProject.Properties.Item("TargetFrameworkMoniker").Value = frameworkDefinition.TargetFramework;
            
            //handle configs
            var projectFolderName = Path.GetFileName(FullPath.TrimEnd('\\'));
            var cfgManager = targetProject.ConfigurationManager;
            foreach (Configuration conf in cfgManager)
            {
                if (conf.ConfigurationName.Contains("Debug") || conf.ConfigurationName.Contains("Release"))
                {
                    var fatherOutputPath = ConfigInfo[conf.ConfigurationName].OutputPath;
                    foreach (var def in MtfSetting.Instance.SupportedFrameworks)
                    {
                        if (fatherOutputPath.EndsWith(def.NugetAbbreviation + "\\"))
                        {
                            fatherOutputPath = fatherOutputPath.Remove(fatherOutputPath.LastIndexOf(def.NugetAbbreviation));
                            break;
                        }
                    }

                    conf.Properties.Item("OutputPath").Value =
                        "..\\" + Path.Combine(fatherOutputPath, frameworkDefinition.NugetAbbreviation) + "\\";
                }
            }


            //handle items
            SetRelativeProjectItems(targetProject.ProjectItems, FullPath);

            //handle references           
                        
            TargetProject[frameworkDefinition] = new ProjectInfo(targetProject);
        }

        public ProjectInfo(Project project)
        {
            IsSourceProject = true;
            IsAddedToSolution = true;

            m_project = project;
            m_solution = (Solution2)project.DTE.Solution;

            Name = project.Name;
            FullName = project.FullName;
            ConfigInfo = new Dictionary<string, ProjectConfigInfo>();
            SupportedPlatforms = new List<string>();

            var cfgManager = project.ConfigurationManager;
            Array platforms = (Array)cfgManager.SupportedPlatforms;
            foreach (var ptf in platforms)
            {
                SupportedPlatforms.Add(ptf.ToString());
            }

            TargetFrameworkMoniker = project.Properties.Item("TargetFrameworkMoniker").Value.ToString();
            OutputFileName = project.Properties.Item("OutputFileName").Value.ToString();
            AssemblyVersion = project.Properties.Item("AssemblyVersion").Value.ToString();
            Description = project.Properties.Item("Description").Value.ToString();
            TargetFramework = project.Properties.Item("TargetFramework").Value.ToString();
            FullPath = project.Properties.Item("FullPath").Value.ToString();
            Company = project.Properties.Item("Company").Value.ToString();
            AssemblyFileVersion = project.Properties.Item("AssemblyFileVersion").Value.ToString();
            LocalPath = project.Properties.Item("LocalPath").Value.ToString();

            FrameworkDefinition = MtfSetting.Instance.SupportedFrameworks.
                First(def => def.TargetFramework == TargetFrameworkMoniker);
            
            foreach (Configuration conf in cfgManager)
            {
                var config = new ProjectConfigInfo();
                config.Name = conf.ConfigurationName;

                config.OutputPath = conf.Properties.Item("OutputPath").Value.ToString();
                if (!config.OutputPath.TrimEnd('\\').EndsWith(FrameworkDefinition.NugetAbbreviation))
                {
                    config.OutputPath = Path.Combine(config.OutputPath, FrameworkDefinition.NugetAbbreviation);
                    conf.Properties.Item("OutputPath").Value = config.OutputPath;
                }
                config.DefineConstants = MtfManager.AddDefineContant(
                    conf.Properties.Item("DefineConstants").Value.ToString(),
                    FrameworkDefinition);
                conf.Properties.Item("DefineConstants").Value = config.DefineConstants;
                config.PlatformTarget = conf.Properties.Item("PlatformTarget").Value.ToString();

                ConfigInfo[conf.ConfigurationName] = config;
            }
            
            ReferenceInfos = new List<ProjectReferenceInfo>();
            VSProject vsProject = (VSProject)project.Object;
            foreach (Reference refe in vsProject.References)
            {
                var reference = new ProjectReferenceInfo();
                reference.Path = refe.Path;
                reference.CopyLocal = refe.CopyLocal;
                if (refe.SourceProject != null)
                {
                    reference.SourceProject = new ProjectInfo(refe.SourceProject);
                }
            }

            ItemInfos = new List<ProjectItemInfo>();
            GetProjectItems(project.ProjectItems);
        }

        private void GetProjectItems(ProjectItems projectItems)
        {
            foreach (ProjectItem pi in projectItems)
            {
                if (pi.ProjectItems.Count > 0)
                {
                    GetProjectItems(pi.ProjectItems);
                }
                else
                {
                    var itemInfo = new ProjectItemInfo();
                    itemInfo.Name = pi.Name;
                    itemInfo.FullPath = pi.Properties.Item("FullPath").Value.ToString();

                    var pros = pi.Properties;
                    if (pros != null)
                    {
                        itemInfo.IsDependentFile = (bool)pros.Item("IsDependentFile").Value;
                        itemInfo.IsLink = (bool)pros.Item("IsLink").Value;
                    }
                }
            }
        }

        private void SetRelativeProjectItems(ProjectItems projectItems, string fatherProjectPath)
        {
            foreach (ProjectItem pi in projectItems)
            {
                if (pi.ProjectItems.Count > 0)
                {
                    SetRelativeProjectItems(pi.ProjectItems, fatherProjectPath);
                }
                else
                {
                    var fatherFullPath = pi.Properties.Item("FullPath").Value.ToString();
                    pi.Properties.Item("FullPath").Value = "..\\" +
                        MtfManager.GetRelativePath(fatherFullPath, fatherProjectPath);
                    var pros = pi.Properties;
                    pros.Item("IsDependentFile").Value = false;
                    pros.Item("IsLink").Value = true;
                }
            }
        }
    }
}
