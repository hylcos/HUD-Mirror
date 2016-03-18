using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace spiegel
{
    class Nos
    {
        private HttpClient httpClient;
        private Uri rssUri;

        public Nos()
        {
            rssUri = new Uri("http://feeds.nos.nl/nosjournaal?format=xml");
            httpClient = new HttpClient();
        }

        public async Task<Headline[]> getHeadlines()
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
