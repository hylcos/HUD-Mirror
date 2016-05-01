using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace spiegel
{
    class WeatherForecast : Widget
    {
        private const String protocol = "http";
        private const String host = "api.openweathermap.org";
        private const int port = 80;
        private HttpClient httpClient;
        private String location, apiKey;
        public WeatherForecast(Grid root,Config config) : base(root,"Weather",config,250,120,new Thickness(10,10,10,10), HorizontalAlignment.Right , VerticalAlignment.Top, TimeSpan.FromMinutes(10))
        {          
            httpClient = new HttpClient();
            
        }
        public async void checkSettings()
        {
            if (!config.hasSetting(name, "location"))
            {
                await config.makeSetting(name, "location", "Amsterdam, NL");
            }
            if (!config.hasSetting(name, "lat"))
            {
                await config.makeSetting(name, "lat", "52.3739206");
            }
            if (!config.hasSetting(name, "long"))
            {
                await config.makeSetting(name, "long", "4.8834042");
            }
            if (!config.hasSetting(name, "units"))
            {
                await config.makeSetting(name, "units", "metric");
            }

        }
        public async override void update()
        {
            if (state)
            {
                Debug.WriteLine("Weather Forecast UPDATE");
                Grid grid = new Grid();
                Forecast forecast = await getForecast();
                //Image
                Image image = new Image();
                BitmapImage bimage = new BitmapImage(new Uri("ms-appx:///Assets/" + forecast.icon + ".png"));
                image.Source = bimage;
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                image.Width = 50;
                image.Height = 50;
                image.Margin = new Thickness(0, 0, 100, 50);

                //Sun(set/rise)
                TextBlock tb = new TextBlock();
                tb.Text = "Sunrise: " + forecast.sunrise.Hour + ":" + forecast.sunrise.Minute + "\n" + "Sunset: " + forecast.sunset.Hour + ":" + forecast.sunset.Minute;
                tb.FontSize = 14;
                tb.Foreground = new SolidColorBrush(Colors.White);
                tb.Margin = new Thickness(0, 55, 0, 0);

                //Temp
                TextBlock temp = new TextBlock();
                temp.Text = "Min: " + forecast.minTemp + "  --  Max: " + forecast.maxTemp;
                temp.FontSize = 10;
                temp.Foreground = new SolidColorBrush(Colors.White);
                temp.Margin = new Thickness(105, 0, 0, 0);

                TextBlock temp2 = new TextBlock();
                temp2.Text = "Gem: " + forecast.temp;
                temp2.FontSize = 10;
                temp2.Foreground = new SolidColorBrush(Colors.White);
                temp2.Margin = new Thickness(0, 20, 0, 0);

                //WindSpeed
                Image wind = new Image();
                BitmapImage windbimage = new BitmapImage(new Uri("ms-appx:///Assets/wind.png"));
                CompositeTransform ts = new CompositeTransform();
                ts.Rotation = Convert.ToDouble(forecast.windDir);
                wind.Source = windbimage;
                wind.RenderTransform = ts;
                wind.RenderTransformOrigin = new Point(0.5, 0.5);
                wind.Width = 50;
                wind.Height = 50;
                wind.Margin = new Thickness(100, 50, 0, 0);

                TextBlock temp3 = new TextBlock();
                temp2.Text = "Location: " + forecast.location;
                temp2.FontSize = 10;
                temp2.Foreground = new SolidColorBrush(Colors.White);
                temp2.Margin = new Thickness(0,100, 0, 0);

                grid.Children.Add(tb);
                grid.Children.Add(image);
                grid.Children.Add(temp);
                grid.Children.Add(temp2);
                grid.Children.Add(temp3);
                grid.Children.Add(wind);

                clearWidget();
                addToWidget(grid);
            }
            else
            {
                clearWidget();
            }      
                                                   
            //Debug.WriteLine("Weather Forecast: " + Marshal.SizeOf(grid));
            
        }
        public async Task<Forecast> getForecast()
        {

            //List<int> forecasts = new List<int>();
            // Forecast[][] forecasts = new Forecast[6][];
            Forecast forecast = null;
            WebUrl webUrl = new WebUrl(protocol, host, port);
            string[] paths = {
                "data",
                "2.5",
                "weather"
            };
            WebUrl.Query[] querys = {
                new WebUrl.Query("lat", config.getSetting(name,"lat")),
                new WebUrl.Query("lon", config.getSetting(name,"long")),
                new WebUrl.Query("mode", "xml"),
                new WebUrl.Query("appid", "11e536b32932b598cfb0b085d19fb203"),
                new WebUrl.Query("units",config.getSetting(name,"units"))
            };
            webUrl.addPath(paths);
            webUrl.addQuery(querys);
            Uri url = webUrl.compose();
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(await httpClient.GetStringAsync(url));
                Debug.WriteLine(xmlDocument.ToString());
                XmlNode element = xmlDocument["current"];
                DateTime sunset = new DateTime(), sunrise= new DateTime();
                String mode = "", icon= "";
                String temp = "", minTemp = "", maxTemp = "";
                String speed = "", direction = "";
                String location = "";
                foreach (XmlNode x in element)
                {
                    XmlAttributeCollection attributes = x.Attributes;
                    switch (x.Name)
                    {
                        case "city":
                            XmlNode city = x["sun"];
                            location = attributes[1].Value;
                            sunrise = Convert.ToDateTime(city.Attributes[0].Value).AddHours(1);
                            sunset = Convert.ToDateTime(city.Attributes[1].Value).AddHours(1);
                            break;
                        case "temperature":
                            temp = attributes[0].Value;
                            minTemp = attributes[1].Value;
                            maxTemp = attributes[2].Value;
                            break;
                        case "weather":
                            mode = attributes[1].Value;
                            icon = attributes[2].Value;
                            break;
                        case "wind":
                            XmlNode speedNode = x["speed"];
                            XmlNode directionNode = x["direction"];
                            speed = speedNode.Attributes[1].Value;
                            direction = directionNode.Attributes[0].Value;
                            break;

                    }
                }
                forecast = new Forecast(sunrise, sunset, mode, icon, direction, speed, temp, minTemp, maxTemp,location);
                
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            return forecast;
        }
    }
}
