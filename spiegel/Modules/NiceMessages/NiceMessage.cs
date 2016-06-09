using System;
using System.Collections.Generic;
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
        public Dictionary<string, Dictionary<int, string>> messages;
        public NiceMessage(Grid UiRoot, Config config) : base(UiRoot, "NiceMessage", config, 1000, 20, new Thickness(0,400,0,0), HorizontalAlignment.Center, VerticalAlignment.Center, TimeSpan.FromMinutes(3))
        {
            string XMLPath = Path.Combine(Package.Current.InstalledLocation.Path + "\\Assets", "XMLFile1.xml");
            XDocument loadedData = XDocument.Load(XMLPath);


        }
        public override async void update()
        {
            if (state)
            {
                Grid grid = new Grid();
                TextBlock tb = new TextBlock();
                tb.Foreground = new SolidColorBrush(Colors.White);
                tb.Text = "Success comes to those who have the will power to win over their snooze buttons. Wishing you an awesome morning.";
                tb.HorizontalAlignment = HorizontalAlignment.Center;
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
