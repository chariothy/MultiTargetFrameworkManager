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
using System.Xml.Linq;

namespace Nu.Vs.Extension
{
    public class ProjectInfo : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public List<ProjectConfigInfo> ConfigInfo { get; set; }        
        public List<string> SupportedPlatforms { get; set; }
        public List<ProjectReferenceInfo> ReferenceInfos { get; set; }
        public List<ProjectItemInfo> ItemInfos { get; set; }
        /// <summary>
        /// Projects which have a definite target framework
        /// </summary>
        public Dictionary<string, ProjectInfo> TargetProject { get; set; }
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
        public ProjectInfo SourceProjectInfo { get; set; }
        public bool IsAddedToSolution { get; set; }

        private Project m_project = null;
        private Solution2 m_solution = null;
        private SolutionInfo m_solutionInfo = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler SupportedFrameworksChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RegisterSupportedFrameworksChangedEvent()
        {
            foreach (var target in MtfSetting.Instance.SupportedFrameworks)
            {
                target.PropertyChanged += target_PropertyChanged;
            }
        }

        void target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RefreshTargetProject();
        }

        public void RemoveTargetProject(FrameworkDefinition frameworkDefinition)
        {
            m_solution.Remove(
                TargetProject.Find(
                    t=>t.FrameworkDefinition.TargetFramework==frameworkDefinition.TargetFramework
                    ).m_project);
        }

        private void CheckAndSetProperty<T>(Property property, T value)
        {
            if (property.Value.ToString() != value.ToString())
            {
                property.Value = value;
            }
        }

        public bool CheckProject(Project project)
        {
            return true;
        }

        public void FindProjectReference()
        {
            ReferenceInfos = new List<ProjectReferenceInfo>();
            VSProject vsProject = (VSProject)project.Object;
            foreach (Reference refe in vsProject.References)
            {
                var reference = new ProjectReferenceInfo();
                reference.Path = refe.Path;
                reference.CopyLocal = refe.CopyLocal;
                if (refe.SourceProject != null)
                {
                    //TODO: if is target , find from target , else find from source
                    var refProjInfo = m_solutionInfo.GetProjectInfo(refe.SourceProject.Name);
                    if (refProjInfo != null)
                    {
                        reference.SourceProject = refProjInfo;
                    }
                    else
                    {
                        if (IsSourceProject)
                        {
                            reference.SourceProject = m_solutionInfo.SourceProject[refProjInfo];
                        }
                        else
                        {
                            reference.SourceProject = m_solutionInfo.GetTargetProjectInfo();
                        }
                    }
                }
            }
        }


        public ProjectInfo AddTargetProject(FrameworkDefinition frameworkDefinition)
        {
            var projectType = Path.GetExtension(FullName);
            var parentOfProjectDir = Directory.GetParent(Directory.GetParent(FullName).FullName).FullName;
            var targetProjectName = Name + MtfSetting.Instance.Separator + frameworkDefinition.NugetAbbreviation;
            var targetProjectDir = Path.Combine(parentOfProjectDir, targetProjectName);
            var targetProjectPath = Path.Combine(targetProjectDir, targetProjectName) + projectType;

            if (!Directory.Exists(targetProjectDir))
            {
                Directory.CreateDirectory(targetProjectDir);
            }

            if(!File.Exists(targetProjectPath))
            {
                File.Copy(FullName, targetProjectPath);
            }

            Project targetProject = null;
            foreach (Project proj in m_solution.Projects)
            {
                if (proj.FullName.ToLower() == targetProjectPath.ToLower())
                {
                    targetProject = proj;
                    break;
                }
            }

            if (targetProject == null)
            {
                //handle items
                SetRelativeProjectItems(
                    targetProjectPath,
                    Path.GetFileName(Path.GetDirectoryName(FullPath))
                );

                targetProject = m_solution.AddFromFile(targetProjectPath); 
            }
            else
            {
                CheckProject(targetProject);
            }

            //handle project property
            CheckAndSetProperty(
                targetProject.Properties.Item("TargetFrameworkMoniker"),
                frameworkDefinition.TargetFramework
            );
            
            //handle configs
            var projectFolderName = Path.GetFileName(FullPath.TrimEnd(Path.DirectorySeparatorChar));
            var cfgManager = targetProject.ConfigurationManager;
            foreach (Configuration conf in cfgManager)
            {
                if (conf.ConfigurationName.Contains("Debug") || conf.ConfigurationName.Contains("Release"))
                {
                    var fatherOutputPath = ConfigInfo.First(inf=>inf.Name==conf.ConfigurationName).OutputPath;
                    foreach (var def in MtfSetting.Instance.SupportedFrameworks)
                    {
                        if (fatherOutputPath.EndsWith(def.NugetAbbreviation + Path.DirectorySeparatorChar))
                        {
                            fatherOutputPath = fatherOutputPath.Remove(fatherOutputPath.LastIndexOf(def.NugetAbbreviation));
                            break;
                        }
                    }

                    CheckAndSetProperty(
                        conf.Properties.Item("OutputPath"),
                         Path.Combine("..", fatherOutputPath, frameworkDefinition.NugetAbbreviation) + Path.DirectorySeparatorChar
                        );
                }
            }


            //handle references                                   
            TargetProject.Add(new ProjectInfo(targetProject, true));
        }

        public ProjectInfo(Project project, SolutionInfo solutionInfo)
        {
            IsSourceProject = true;
            IsAddedToSolution = true;

            m_project = project;
            m_solution = (Solution2)project.DTE.Solution;
            m_solutionInfo = solutionInfo;

            Name = project.Name;
            FullName = project.FullName;
            ConfigInfo = new List<ProjectConfigInfo>();
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
                if (!config.OutputPath.TrimEnd(Path.DirectorySeparatorChar).EndsWith(FrameworkDefinition.NugetAbbreviation))
                {
                    config.OutputPath = Path.Combine(config.OutputPath, FrameworkDefinition.NugetAbbreviation);
                    conf.Properties.Item("OutputPath").Value = config.OutputPath;
                }
                var defineConstants = conf.Properties.Item("DefineConstants").Value.ToString();
                config.DefineConstants = MtfManager.AddDefineConstants(defineConstants, FrameworkDefinition);
                conf.Properties.Item("DefineConstants").Value = config.DefineConstants;
                config.PlatformTarget = conf.Properties.Item("PlatformTarget").Value.ToString();

                ConfigInfo.Add(config);
            }
            

            

            ItemInfos = new List<ProjectItemInfo>();
            GetProjectItems(project.ProjectItems);

            RegisterSupportedFrameworksChangedEvent();

            TargetProject = new Dictionary<string, ProjectInfo>();
        }

        public void RefreshTargetProject()
        {
            foreach (var target in MtfSetting.Instance.SupportedFrameworks)
            {
                var containsTarget = TargetProject.ContainsKey(target.TargetFramework);
                if (containsTarget && !target.Selected)
                {
                    //TODO: remove from solution and containsTarget
                }
                else if(target.Selected && !containsTarget)
                {
                    AddTargetProject(target);
                }
                else if (containsTarget && target.Selected)
                {
                    CheckTargetProject(target);
                }
            }
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

        private void SetRelativeProjectItems(string projectPath, string fatherProjectFolder)
        {
            var doc = XDocument.Load(projectPath);
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var elems = doc.Descendants(ns + "Project")
                          .Where(t => t.Attribute("ToolsVersion") != null)
                          .Elements(ns + "ItemGroup")
                          .Elements(ns + "Compile")
                          .Where(r => r.Attribute("Include") != null);
            foreach (var elem in elems)
            {
                var ori = elem.FirstAttribute.Value;
                elem.FirstAttribute.SetValue(Path.Combine("..", fatherProjectFolder, ori));
                Console.WriteLine(elem.FirstAttribute.Value);
            }
            doc.Save(projectPath);

            /*
            foreach (ProjectItem pi in projectItems)
            {
                if (pi.ProjectItems.Count > 0)
                {
                    SetRelativeProjectItems(pi.ProjectItems, Path.Combine(leadingDir, pi.Name), fatherProjectPath);
                }
                else
                {
                    var projectFolder = Path.GetDirectoryName(pi.ContainingProject.FullName);
                    var realItemPath = Path.Combine(fatherProjectPath, pi.Name);
                    var relativeItemPath = MtfManager.GetRelativePath(realItemPath, projectFolder);
                    CheckAndSetProperty(pi.Properties.Item("FullPath"), relativeItemPath);
                    var pros = pi.Properties;
                    pros.Item("IsDependentFile").Value = false;
                    pros.Item("IsLink").Value = true;
                }
            }
             * */
        }
    }
}
