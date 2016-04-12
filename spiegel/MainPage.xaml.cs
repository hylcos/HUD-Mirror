using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
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
        private Dictionary<string,bool> modules;
        private List<String> moduleNames;
        private Clock clock;
        private Nos nosFeed;
        private GCal gCal;
        private WeatherForecast weatherData;

        private List<Updateable> updateables;
        private StreamSocketListener socketListener;

        public MainPage()
        {
            this.InitializeComponent();
            //checkBoot();
            initializeNetwork();

            //initializeHud();
        }

        private async void initializeNetwork()
        {
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += OnConnection;
            socketListener.Control.KeepAlive = true;
            try
            {
                await socketListener.BindServiceNameAsync("8080");

                Debug.WriteLine("Server Started");

            }
            catch (Exception e)
            {
                Debug.WriteLine("Cant Start a Socket: " + e);
            }
        }

        private async void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            DataReader reader = new DataReader(args.Socket.InputStream);
            DataWriter writer = new DataWriter(args.Socket.OutputStream);
            try
            {
                Debug.WriteLine("Made a connection");
                while (true)
                {
                    writer.WriteString("Hello World");
                    // Read first 4 bytes (length of the subsequent string).
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    Debug.WriteLine(reader.ReadString(sizeFieldCount));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        // The underlying socket was closed before we were able to read the whole data.
                        return;
                    }

                    // Read the string.
                    uint stringLength = reader.ReadUInt32();
                    uint actualStringLength = await reader.LoadAsync(stringLength);
                    Debug.WriteLine(reader.ReadString(actualStringLength));
                    if (stringLength != actualStringLength)
                    {
                        // The underlying socket was closed before we were able to read the whole data.
                        return;
                    }
                    


                }
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
                
            }
           
            //Read line from the remote client.
            /*Stream inStream = args.Socket.InputStream.AsStreamForRead();
            StreamReader reader = new StreamReader(inStream);
            string request = await reader.ReadLineAsync();

            //Send the line back to the remote client.
            Stream outStream = args.Socket.OutputStream.AsStreamForWrite();
            StreamWriter writer = new StreamWriter(outStream);
            await writer.WriteLineAsync(request);
            await writer.FlushAsync();*/
        }

        private async void checkBoot()
        {
            moduleNames = new List<string>();
            modules = new Dictionary<string, bool>();

            moduleNames.Add("Clock");
            moduleNames.Add("GoogleCalendar");
            moduleNames.Add("News");
            moduleNames.Add("Weather");


            uiRoot = root;
            config = new Config();
            showUnableToStartMessage(uiRoot, "First Boot");
            try
            {
                await config.LoadFromFile();
            }
            catch (UnableToReadConfigurationFileException e)
            {
                //kan config file niet lezen, programma mag niet verder gaan! (kan wel een foutmelding weergeven in GUI)
                showUnableToStartMessage(uiRoot, e.Message);
                firstBoot();
                return;
            }
            catch (UnableToAsignConfigurationSettingsException e)
            {
                //kan config file niet parsen, programma mag niet verder gaan! (kan wel een foutmelding weergeven in GUI)
                showUnableToStartMessage(uiRoot, e.Message);
                return;
            }
            catch (Exception e)
            {

            }
        }

        private void firstBoot()
        {
            foreach(string moduleName in moduleNames)
            {
                modules[moduleName] = false;
            }
        }

        private async void initializeHud()
        {
            
            updateables = new List<Updateable>();


            clock = new Clock(uiRoot);
            updateables.Add(clock);

            nosFeed = new Nos(uiRoot);
            updateables.Add(nosFeed);


            gCal = new GCal(config.settings[Config.ConfigType.googleCalendarKey], uiRoot,config.settings[Config.ConfigType.googleRefreshKey]);//config.settings[Config.ConfigType.googleRefreshKey]); //"AIzaSyDNV7ivdpJI0UHZYYD56YIpBrIupRISN2A"
            updateables.Add(gCal);

            weatherData = new WeatherForecast("11e536b32932b598cfb0b085d19fb203", "Nieuwegein,nl",uiRoot);
            updateables.Add(weatherData);

            //er wordt een thread aangemaakt voor alle updateables
            foreach (Updateable updateable in updateables)
            {
                updateThread(updateable);
            }
        }


        private async void updateThread(Updateable updateable)
        {
             while (true)
            {
                updateable.update();  
                //await Task.Delay(TimeSpan.FromSeconds(1));        
                await Task.Delay(updateable.updatePeriod);
                
            }
            throw new Exception();
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
