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
using Windows.Data.Json;
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
            checkBoot();
            initializeNetwork();

            initializeHud();
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

        public async Task<String> getModules()
        {
            String value = "{";
            value += "\"commando\":\"giveModules\",";
            value += "\"modules\": [";
            foreach (KeyValuePair<string, bool> module in modules)
            {
                if (value.Last() == '}')
                {
                    value += ",";
                }
                value += "{\"moduleName\":\"" + module.Key + "\",\"value\":" + ((module.Value) ? "true" : "false") + "}";
            }
            value += "]}";
            return value;
        }

        public async Task<String> getSettings(String moduleNameSettings)
        {
            String value = "{";
            value += "\"commando\":\"giveSettings\",";
            value += "\"module\":\""+moduleNameSettings+"\",";
            value += "\"settings\": [";
            foreach (KeyValuePair<string, string> setting in config.getModuleSettings(moduleNameSettings))
            {
                if (value.Last() == '}')
                {
                    value += ",";
                }
                value += "{\""+setting.Key+" \":\"" + setting.Value + "\"}";
            }
            value += "]}";
            return value;
        }
        private async void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try {
                while (true) {
                    Stream inStream = args.Socket.InputStream.AsStreamForRead();
                    StreamReader reader = new StreamReader(inStream);
                    string request = await reader.ReadLineAsync();
                    Stream outStream = args.Socket.OutputStream.AsStreamForWrite();
                    StreamWriter writer = new StreamWriter(outStream);

                    if(request == null)
                    {
                        break;
                    }
                    try
                    {
                        JsonObject jsonObject = JsonObject.Parse(request);
                        String commando = jsonObject.GetNamedString("commando");
                        String moduleName,value,setting;
                        switch (commando)
                        {
                            case "getModules":
                                String modulesJson = await getModules();
                                Debug.WriteLine(modulesJson);
                                await writer.WriteLineAsync(modulesJson + "\n");
                                await writer.FlushAsync();
                                break;
                            case "getSettings":
                                moduleName = jsonObject.GetNamedString("module");
                                String settingsJson = await getSettings(moduleName);
                                Debug.WriteLine(settingsJson);
                                await writer.WriteLineAsync(settingsJson + "\n");
                                await writer.FlushAsync();
                                break;
                            case "setSetting":
                                moduleName = jsonObject.GetNamedString("module");
                                setting = jsonObject.GetNamedString("setting");
                                value = jsonObject.GetNamedString("value");
                                break;
                            case "enableModule":
                                moduleName = jsonObject.GetNamedString("module");
                                config.setSetting(moduleName, "enabled", "true");
                                //updateEnabled(moduleName);
                                break;
                            case "disableModule":
                                moduleName = jsonObject.GetNamedString("module");
                                config.setSetting(moduleName, "enabled", "false");
                                //updateEnabled(moduleName);
                                break;
                        }
                    } catch(Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                    //Send the line back to the remote client. 

                    Debug.WriteLine(request);
                   
                }
            }catch(Exception e)
            {
                //Debug.WriteLine(e.ToString());
            }
        }

        /*private void updateEnabled(string moduleName)
        {
            switch (moduleName)
            {
                case "Clock":
                    clock.updateEnabled();
                    break;
                case "GoogleCalendar":
                    gCal.updateEnabled();
                    break;
                case "News":
                    nosFeed.updateEnabled();
                    break;
                case "Weather":
                    weatherData.updateEnabled();
                    break;
            }
        }*/

        private async void checkBoot()
        {
            moduleNames = new List<string>();
            modules = new Dictionary<string, bool>();

            moduleNames.Add("Clock");
            moduleNames.Add("GoogleCalendar");
            moduleNames.Add("News");
            moduleNames.Add("Weather");


            uiRoot = root;
            config = new Config(moduleNames);
            //showUnableToStartMessage(uiRoot, "First Boot");
            try
            {
                await config.LoadFromFile();
                foreach(string moduleName in moduleNames)
                {
                    modules[moduleName] = (config.getSetting(moduleName,"enabled") == "true"? true : false);
                }
            }
            catch (UnableToReadConfigurationFileException e) { 
            
                //kan config file niet lezen, programma mag niet verder gaan! (kan wel een foutmelding weergeven in GUI)
                //showUnableToStartMessage(uiRoot, e.Message);
                await firstBoot();
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

        private async Task firstBoot()
        {
            foreach(string moduleName in moduleNames)
            {
                modules[moduleName] = false;
            }
            Debug.WriteLine("Making new file");
            await config.makeFile(moduleNames);
        }

        private async void initializeHud()
        {
            
            updateables = new List<Updateable>();


            clock = new Clock(uiRoot,config);
            clock.checkSettings();
            updateables.Add(clock);

            nosFeed = new Nos(uiRoot,config);
            updateables.Add(nosFeed);


            //gCal = new GCal(config.settings[Config.ConfigType.googleCalendarKey], uiRoot,config.settings[Config.ConfigType.googleRefreshKey]);//config.settings[Config.ConfigType.googleRefreshKey]); //"AIzaSyDNV7ivdpJI0UHZYYD56YIpBrIupRISN2A"
            //updateables.Add(gCal);

            weatherData = new WeatherForecast(uiRoot,config);
            weatherData.checkSettings();
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
                double time = updateable.updatePeriod.TotalSeconds;
                while (time > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    if (((Widget)updateable).updateEnabled())
                    {
                        time = 0;
                    }
                    time--;
                }       
                
            }
            //throw new Exception();
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
