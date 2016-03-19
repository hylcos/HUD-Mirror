using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace spiegel
{
    class Nos: Widget
    {
        private HttpClient httpClient;
        private Uri rssUri;

        public Nos(Grid UiRoot) : base(UiRoot, 500, 600, new Thickness(10, 10, 10, 10), HorizontalAlignment.Center, VerticalAlignment.Center, TimeSpan.FromSeconds(3))
        {
            rssUri = new Uri("http://feeds.nos.nl/nosjournaal?format=xml");
            httpClient = new HttpClient();
        }


        public override async void update()
        {
            Headline[] headlines = await getHeadlines();

            clearWidget();

            //todo: data in widget tonen dmv UI elements
        }

        private async Task<Headline[]> getHeadlines()
        {
            try {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(await httpClient.GetStringAsync(rssUri));
                String xmlString = xmlDocument.ToString();
                return parseHeadlinesFromXml(xmlDocument).ToArray();
            }
            catch(Exception e)
            {
                throw new UnableToParseFeedException();
            }
        }

        private List<Headline> parseHeadlinesFromXml (XmlDocument xmlData) {
            List<Headline> headlines = new List<Headline>();

            XmlNodeList element = xmlData.GetElementsByTagName("channel");

            foreach (XmlNode xn in element)
            {
                foreach(XmlNode x in xn)
                {
                    if (x.Name.Equals("item"))
                    {
                        headlines.Add(new Headline(x["title"].InnerText, string.Empty, DateTime.Parse(x["pubDate"].InnerText)));
                    }
                }
            }

            //haal juiste info uit XML

            return headlines;
        }
    }
}
