using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Nu.Vs.Extension
{
    public class MtfSetting
    {
        public static MtfSetting Instance = new MtfSetting();
        private static string s_settingFilePath = "Setting.json";
        public char Separator { get; set; }
        public ObservableCollection<FrameworkDefinition> SupportedFrameworks { get; set; }
        public string SelectedFramework
        {
            get
            {
                var sb = new StringBuilder();
                var first = true;
                foreach (var item in SupportedFrameworks)
                {
                    if (item.Selected)
                    {
                        if (!first)
                        {
                            sb.Append(";");                            
                        }
                        else
                        {
                            first = false;
                        }
                        sb.Append(item.NugetAbbreviation);
                    }
                }
                return sb.ToString();
            }
        }

        

        static MtfSetting()
        {
            var serializer = new JavaScriptSerializer();
            if (File.Exists(s_settingFilePath))
            {
                using (var stream = new StreamReader(s_settingFilePath))
                {
                    Instance = serializer.Deserialize<MtfSetting>(stream.ReadToEnd());
                }
            }
            else
            {
                using (var stream = new StreamWriter(s_settingFilePath))
                {
                    var json = serializer.Serialize(Instance);
                    var jsonPretty = JSON_PrettyPrinter.Process(json);
                    stream.Write(jsonPretty);
                }
            }
        }

        

        public MtfSetting()
        {
            Separator = '_';
            SupportedFrameworks =
                new ObservableCollection<FrameworkDefinition>
                {
                      new FrameworkDefinition("net452", ".NETFramework,Version=v4.5.2", false),
                      new FrameworkDefinition("net451", ".NETFramework,Version=v4.5.1", false),
                      new FrameworkDefinition("net45", ".NETFramework,Version=v4.5", true),
                      new FrameworkDefinition("net40", ".NETFramework,Version=v4.0", true),
                      new FrameworkDefinition("net40-client", ".NETFramework,Version=v4.0,Profile=Client", false),
                      new FrameworkDefinition("net35", ".NETFramework,Version=v3.5", true),
                      new FrameworkDefinition("net35-client", ".NETFramework,Version=v3.5,Profile=Client", false),
                      new FrameworkDefinition("net30", ".NETFramework,Version=v3.0", false),
                      new FrameworkDefinition("net20", ".NETFramework,Version=v2.0", true)
                };
        }
    }
}
