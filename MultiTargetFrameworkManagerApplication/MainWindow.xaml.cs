using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Nu.Vs.Extension;

namespace MultiTargetFrameworkManagerApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MtfManager m_mtfManager = new MtfManager();
        public MainWindow()
        {
            InitializeComponent();

            OpenedSolutionListBox.ItemsSource = SolutionInfo.GetOpenedSolutions();            
            SupportedFrameworksComboBox.DataContext = MtfSetting.Instance;
        }

        private void SupportedFrameworksComboBox_DropDownClosed(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var setting = comboBox.DataContext as MtfSetting;
                if (setting != null)
                {
                    comboBox.Text = setting.SelectedFramework;
                }
            }
        }

        private void OpenedSolutionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = e.Source as ComboBox;

            if (comboBox != null)
            {
                var selectedSolution = comboBox.SelectedItem as SolutionInfo;
                selectedSolution.Initialize();
                SolutionInfoGrid.DataContext = selectedSolution;
            }
        }
    }
}
