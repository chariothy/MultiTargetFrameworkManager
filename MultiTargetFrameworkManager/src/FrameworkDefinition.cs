using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nu.Vs.Extension
{
    public class FrameworkDefinition : INotifyPropertyChanged
    {
        public string NugetAbbreviation { get; set; }
        public string TargetFramework { get; set; }
        private bool m_selected = false;

        public bool Selected
        {
            get { return m_selected; }
            set 
            {
                if (m_selected != value)
                {
                    m_selected = value;
                    this.OnPropertyChanged("Selected");
                }
            }
        }

        public FrameworkDefinition()
        {

        }

        public FrameworkDefinition(FrameworkDefinition rhs)
            :this(rhs.NugetAbbreviation, rhs.TargetFramework, rhs.Selected)
        {

        }

        public FrameworkDefinition Clone()
        {
            return new FrameworkDefinition(this);
        }

        public FrameworkDefinition(string nugetAbbr, string targetFramework, bool selected)
        {
            NugetAbbreviation = nugetAbbr;
            TargetFramework = targetFramework;
            Selected = selected;
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
    }
}
