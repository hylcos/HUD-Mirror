using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace spiegel
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Config config;

        private Nos nosFeed;
        private GCal gCal;


        private int i = 0;
        public MainPage()
        {
            this.InitializeComponent();

            config = new Config();
            


            nosFeed = new Nos();
            gCal = new GCal("AIzaSyDNV7ivdpJI0UHZYYD56YIpBrIupRISN2A");

            updateCalendarThread();
            updateTempThread(); 
        }

        private async void updateTempThread()
        {

            try {
                Headline[] nosHeadlines = await nosFeed.getHeadlines();
                temperature.Text = nosHeadlines.First().title;
            }
            catch(UnableToParseFeedException e)
            {

            }
        }
        private async void updateCalendarThread()
        {
            await config.LoadFromFile();
            try
            {
                

                CalendarItem[] calendarItems = await gCal.getLatestItems();
                String text = "";
                foreach(CalendarItem item in calendarItems)
                {
                    text += item.ToString() + "\n";
                }
                CalBox.Text = text;
            }
            catch (UnableToParseFeedException e)
            {

            }
        }
    }
}
