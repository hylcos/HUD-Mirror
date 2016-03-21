using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace spiegel
{
    class WeatherForecast
    {
        private const String protocol = "http";
        private const String host = "api.openweathermap.org";
        private const int port = 80;
        private HttpClient httpClient;
        private String location, apiKey;
        public WeatherForecast(String apiKey,String location)
        {
            
            this.apiKey = apiKey;
            this.location = location;
            httpClient = new HttpClient();
            
        }
        public async Task<Forecast[]> getForecast()
        {
            List<Forecast> forecasts = new List<Forecast>();
            WebUrl webUrl = new WebUrl(protocol, host, port);
            string[] paths = {
                "data",
                "2.5",
                "forecast"
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

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(await httpClient.GetStringAsync(url));
                XmlNode element = xmlDocument["weatherdata"].GetElementsByTagName("forecast")[0];
                foreach (XmlNode x in element)
                {
                    XmlAttributeCollection attributes = x.Attributes;

                    DateTime startDate, endDate;
                    String type = "", windDir = "", windspeed = "",temp = "";
                    startDate = Convert.ToDateTime(attributes[0].Value);
                    endDate = Convert.ToDateTime(attributes[1].Value);
                    foreach (XmlNode xn in x)
                    {
                        XmlAttributeCollection xnAttributes = xn.Attributes;
                        switch (xn.Name)
                        {
                            case "symbol":
                                type = xnAttributes[1].Value;
                                break;
                            case "windDirection":
                                windDir = xnAttributes[1].Value;
                                break;
                            case "windSpeed":
                                windspeed = xnAttributes[1].Value;
                                break;
                            case "temperature":
                                temp = xnAttributes[1].Value;
                                break;
                                
                        }
                    }
                    forecasts.Add(new Forecast(startDate, endDate, type, windDir, windspeed,temp));
                }
            }
            catch(Exception e)
            {

            }
            return forecasts.ToArray();
        }
    }
}
