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
using Windows.UI;
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
        private Grid uiRoot;
        private Config config;

        private Clock clock;
        private Nos nosFeed;
        private GCal gCal;
        private WeatherForecast weatherData;

        private List<Updateable> updateables;
        

        public MainPage()
        {
            this.InitializeComponent();
            initializeHud();
        }


        private async void initializeHud()
        {
            uiRoot = root;


            config = new Config();
            try {
                await config.LoadFromFile();
            }
            catch (UnableToReadConfigurationFileException e)
            {
                //kan config file niet lezen, programma mag niet verder gaan! (kan wel een foutmelding weergeven in GUI)
                showUnableToStartMessage(uiRoot, e.Message);
                return;
            }
            catch (UnableToAsignConfigurationSettingsException e)
            {
                //kan config file niet parsen, programma mag niet verder gaan! (kan wel een foutmelding weergeven in GUI)
                showUnableToStartMessage(uiRoot, e.Message);
                return;
            }


            updateables = new List<Updateable>();


            clock = new Clock(uiRoot);
            updateables.Add(clock);

            nosFeed = new Nos(uiRoot);
            updateables.Add(nosFeed);


            gCal = new GCal(config.settings[Config.ConfigType.googleCalendarKey], uiRoot); //"AIzaSyDNV7ivdpJI0UHZYYD56YIpBrIupRISN2A"
            updateables.Add(gCal);

            weatherData = new WeatherForecast("11e536b32932b598cfb0b085d19fb203", "Nieuwegein,nl");

  
            //er wordt een thread aangemaakt voor alle updateables
            foreach(Updateable updateable in updateables)
            {
                updateThread(updateable);
            }
        }


        private async void updateThread(Updateable updateable)
        {
            while (true)
            {
                updateable.update();          
                await Task.Delay(updateable.updatePeriod);
            }
        }


        private void showUnableToStartMessage(Grid uiRoot, String message)
        {
            TextBlock errorMessage = new TextBlock();
            errorMessage.Text = "Het is niet gelukt om de HUD te starten..." + "\n" + "{" + message + "}";
            errorMessage.Foreground = new SolidColorBrush(Colors.White);
            errorMessage.HorizontalAlignment = HorizontalAlignment.Center;
            errorMessage.VerticalAlignment = VerticalAlignment.Center;
            errorMessage.TextAlignment = TextAlignment.Center;
            errorMessage.FontSize = 40;

            uiRoot.Children.Clear();
            uiRoot.Children.Add(errorMessage);
        }
    }
}
