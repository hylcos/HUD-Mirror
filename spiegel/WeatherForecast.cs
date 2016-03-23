using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public WeatherForecast(String apiKey,String location,Grid root) : base(root,200,120,new Thickness(10,10,10,10), HorizontalAlignment.Right , VerticalAlignment.Top, TimeSpan.FromHours(3))
        {            
            this.apiKey = apiKey;
            this.location = location;
            httpClient = new HttpClient();
            
        }
        public async override void update()
        {
            Grid grid = new Grid();
            Forecast forecast = await getForecast();
            //Image
            Image image = new Image();
            BitmapImage bimage = new BitmapImage(new Uri("ms-appx:///Assets/"+forecast.icon+".png"));
            image.Source = bimage;
            image.RenderTransformOrigin = new Point(0.5,0.5);
            image.Width = 50;
            image.Height = 50;
            image.Margin = new Thickness(0, 0, 100, 50);

            //Sun(set/rise)
            TextBlock tb = new TextBlock();
            tb.Text = "Sunrise: " + forecast.sunrise.Hour+":" +forecast.sunrise.Minute + "\n" + "Sunset: " + forecast.sunset.Hour + ":" + forecast.sunset.Minute;
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
            temp2.FontSize = 20;
            temp2.Foreground = new SolidColorBrush(Colors.White);
            temp2.Margin = new Thickness(100,20, 0, 0);

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
            wind.Margin = new Thickness(100, 50, 0, 0 );



            grid.Children.Add(tb);            
            grid.Children.Add(image);
            grid.Children.Add(temp);
            grid.Children.Add(temp2);
            grid.Children.Add(wind);


            addToWidget(grid);
            
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
                new WebUrl.Query("q", location),
                new WebUrl.Query("mode", "xml"),
                new WebUrl.Query("appid", apiKey),
                new WebUrl.Query("units","metric")
            };
            webUrl.addPath(paths);
            webUrl.addQuery(querys);
            Uri url = webUrl.compose();
            int i = -1;
            int oldDate = -1;
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(await httpClient.GetStringAsync(url));
                XmlNode element = xmlDocument["current"];
                DateTime sunset = new DateTime(), sunrise= new DateTime();
                String mode = "", icon= "";
                String temp = "", minTemp = "", maxTemp = "";
                String speed = "", direction = "";

                foreach (XmlNode x in element)
                {
                    XmlAttributeCollection attributes = x.Attributes;
                    switch (x.Name)
                    {
                        case "city":
                            XmlNode city = x["sun"];
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
                forecast = new Forecast(sunrise, sunset, mode, icon, direction, speed, temp, minTemp, maxTemp);
/*
                    
                    String type = "", windDir = "", windspeed = "",temp = "";
                    sunrise = Convert.ToDateTime(attributes[0].Value);
                    endDate = Convert.ToDateTime(attributes[1].Value);
                    if(startDate.Day != oldDate)
                    {
                        oldDate = startDate.Day;
                        i++;
                    }

                    foreach (XmlNode xn in x)
                    {
                        XmlAttributeCollection xnAttributes = xn.Attributes;
                       
                    }
                   
                    if(forecasts[i] != null)
                    {
                        forecasts[i] = new Forecast[8];
                    }*/
                    //forecasts[i][startDate.Hour / 3] = new Forecast(startDate, endDate, type, windDir, windspeed,temp);

                
            }
            catch(Exception e)
            {

            }
            return forecast;
        }
    }
}
