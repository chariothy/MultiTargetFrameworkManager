using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using EnvDTE;
using EnvDTE80;

namespace Nu.MultiTargetFramework
{
    public class ProjectInfo : INotifyPropertyChanged
    {
        private string m_Info;

        public string Info
        {
            get { return m_Info; }
            set
            {
                if (m_Info != value)
                {
                    m_Info = value;
                    OnPropertyChanged("Info");
                }
            }
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

        public void GetProjectInfo()
        {
            EnvDTE80.DTE2 dte2 = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.
                    GetActiveObject("VisualStudio.DTE.12.0");

            Solution2 slu2 = (Solution2)dte2.Solution;
            foreach (Project proj in slu2.Projects)
            {
                Info += proj.FullName + "\n";
            }
            
        }
    }
}
