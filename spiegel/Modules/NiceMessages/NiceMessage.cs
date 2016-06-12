using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace spiegel
{
    class NiceMessage : Widget
    {
        private Dictionary<string,List<string>> messages;
        private int night = 5, morning = 12, afternoon = 17, evening = 24;
        public NiceMessage(Grid UiRoot, Config config) : base(UiRoot, "NiceMessage", config, 1000, 40, new Thickness(0,400,0,0), HorizontalAlignment.Center, VerticalAlignment.Center, TimeSpan.FromSeconds(5))
        {
            string XMLPath = Path.Combine(Package.Current.InstalledLocation.Path + "\\Assets", "XMLFile1.xml");
            XDocument loadedData = XDocument.Load(XMLPath);
            messages = new Dictionary<string, List<string>>();
            XElement firstNode = loadedData.Root;
            foreach (XElement node in firstNode.Elements())
            {
                messages[node.Name.ToString()] = new List<string>();
                Debug.WriteLine(node.Name.ToString());
                foreach (XElement _node in node.Elements())
                {
                    foreach (XElement __node in _node.Elements())
                        {
                            messages[node.Name.ToString()].Add(__node.Value);
                        }
                    
                }
            }

            


        }
        public override async void update()
        {
            if (state)
            {
                string message = "No Message Today!";
                Random rnd = new Random();
                DateTime dateTime = DateTime.Now;
                if (dateTime.Hour < night)
                {
                    message = messages["night"][rnd.Next(0, messages["night"].Count)];
                }
                else if (dateTime.Hour < morning)
                {
                    message = messages["morning"][rnd.Next(0, messages["morning"].Count)];
                }
                else if (dateTime.Hour < afternoon)
                {
                    message = messages["afternoon"][rnd.Next(0,messages["afternoon"].Count)];
                }
                else if (dateTime.Hour < evening)
                {
                    message = messages["evening"][rnd.Next(0, messages["evening"].Count)];
                }
                Grid grid = new Grid();
                TextBlock tb = new TextBlock();
                tb.Foreground = new SolidColorBrush(Colors.White);
                tb.Text = message;
                tb.TextWrapping = TextWrapping.WrapWholeWords;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                clearWidget();
                grid.Children.Add(tb);
            
                addToWidget(grid);
            }
            else
            {
                clearWidget();
            }
        }
    }
    
}
