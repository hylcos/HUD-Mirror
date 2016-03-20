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
    class Nos : Widget
    {
        private HttpClient httpClient;
        private Uri rssUri;

        public Nos(Grid UiRoot) : base(UiRoot, 300, 900, new Thickness(10,110, 10, 10), HorizontalAlignment.Right, VerticalAlignment.Top, TimeSpan.FromSeconds(3))
        {
            rssUri = new Uri("http://feeds.nos.nl/nosjournaal?format=xml");
            httpClient = new HttpClient();
        }


        public override async void update()
        {
            Headline[] headlines = await getHeadlines();

            clearWidget();
            Grid grid = new Grid();
            grid.Height = 900;
            grid.Width = 300;
            int i = 0;
            foreach(Headline hl in headlines)
            {
                TextBlock tb = new TextBlock();
                tb.FontSize = 10;
                tb.Foreground = new SolidColorBrush(Colors.White);
                tb.TextAlignment = TextAlignment.Left;
                tb.Text = hl.ToString();
                tb.Margin = new Thickness(0, (tb.FontSize*3.5) * i, 0, 0);
                tb.TextWrapping = TextWrapping.WrapWholeWords;
                grid.Children.Add(tb);
                i++;
            }
            addToWidget(grid);
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
                        headlines.Add(new Headline(x["title"].InnerText, x["description"].InnerText, DateTime.Parse(x["pubDate"].InnerText)));
                    }
                }
            }

            //haal juiste info uit XML

            return headlines;
        }
    }
}
