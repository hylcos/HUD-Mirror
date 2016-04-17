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
using Windows.UI.Xaml.Shapes;

namespace spiegel
{
    class Nos : Widget
    {
        public class ScrollSlot
        {
            public int position { get; set; }
            public int lastPosition { get; set; }
            public Headline headline { get; set; }

            public ScrollSlot(int position, Headline headline)
            {
                this.position = position;
                this.headline = headline;
                lastPosition = position;
            }
        }

        private const int fontsize = 20;
        private const int margin = 10;

        private int maxScrollSlots;
        private const String protocol = "http";
        private const String host = "feeds.nos.nl";
        private const int port = 80;


        private HttpClient httpClient;
        private WebUrl webUrl;
        private Uri rssUri;

        private Headline[] headlines = { };

        private Grid headlineBox;
        private List<ScrollSlot> scrollSlots;

        public Nos(Grid UiRoot,Config config) : base(UiRoot, "News",config,800, 300, new Thickness(10,110, 10, 10), HorizontalAlignment.Left, VerticalAlignment.Top, TimeSpan.FromSeconds(30))
        {
            maxScrollSlots = ((int)widgetBox.Height / (fontsize + margin)) + 1;
            webUrl = new WebUrl(protocol, host, port);
            string[] paths = {
                "nosjournaal"
            };
            WebUrl.Query[] querys = {
                new WebUrl.Query("format", "xml")
            };
            webUrl.addPath(paths);
            webUrl.addQuery(querys);
            rssUri = webUrl.compose();
            httpClient = new HttpClient();

            clearWidget(); //just to be sure

            headlineBox = new Grid();

            Color transBlack = new Color();
            transBlack.A = 0;
            transBlack.R = 0;
            transBlack.G = 0;
            transBlack.B = 0;

            LinearGradientBrush linearTop = new LinearGradientBrush();
            linearTop.StartPoint = new Point(0.5, 0);
            linearTop.EndPoint = new Point(0.5, 1);
            linearTop.GradientStops.Add(new GradientStop() { Color = Colors.Black, Offset = 0.0 });
            linearTop.GradientStops.Add(new GradientStop() { Color = transBlack, Offset = 1.0 });

            Rectangle topFade = new Rectangle();
            topFade.Height = 30;
            topFade.VerticalAlignment = VerticalAlignment.Top;
            topFade.Fill = linearTop;


            LinearGradientBrush linearBottom = new LinearGradientBrush();
            linearBottom.StartPoint = new Point(0.5, 0);
            linearBottom.EndPoint = new Point(0.5, 1);
            linearBottom.GradientStops.Add(new GradientStop() { Color = Colors.Black, Offset = 1.0 });
            linearBottom.GradientStops.Add(new GradientStop() { Color = transBlack, Offset = 0.0 });

            Rectangle bottomFade = new Rectangle();
            bottomFade.Height = 30;
            bottomFade.VerticalAlignment = VerticalAlignment.Bottom;
            bottomFade.Fill = linearBottom;

            Rectangle topHide = new Rectangle();
            topHide.Height = margin + fontsize;
            topHide.VerticalAlignment = VerticalAlignment.Top;
            topHide.Margin = new Thickness(0, -(margin + fontsize), 0, 0);
            topHide.Fill = new SolidColorBrush(Colors.Black);

            addToWidget(headlineBox);

            addToWidget(topHide);
            addToWidget(topFade);
            addToWidget(bottomFade);

            ScrollThread();
        }


        public override async void update()
        {
            headlineBox.Children.Clear();

            headlines = await getHeadlines();
        }


        private async void ScrollThread()
        {
            int scrollPostion = 0;
            Queue<Headline> headlineQueue = new Queue<Headline>();
            scrollSlots = new List<ScrollSlot>();
            for (byte i = 0; i < maxScrollSlots; i++)
            {
                ScrollSlot scrollslot = new ScrollSlot(i * (fontsize + margin), new Headline(string.Empty, string.Empty, new DateTime()));
                scrollSlots.Add(scrollslot);
            }


            while (true)
            {
                if(headlineQueue.Count == 0)
                {
                    foreach(Headline hl in headlines)
                    {
                        headlineQueue.Enqueue(hl);
                    }
                }

                headlineBox.Children.Clear();
                
                foreach(ScrollSlot sl in scrollSlots) {
                    int currentPosition = sl.lastPosition;
                    int newPosition = ((sl.position + scrollPostion) % ((int)widgetBox.Height + (fontsize))) - (fontsize + margin);

                    if (newPosition < currentPosition && headlineQueue.Count > 0) //slot is naar boven gesprongen
                    {
                        sl.headline = headlineQueue.Dequeue();
                    }

                    TextBlock tb = new TextBlock();
                    tb.FontSize = fontsize;
                    tb.Foreground = new SolidColorBrush(Colors.White);
                    tb.TextAlignment = TextAlignment.Left;
                    tb.Text = sl.headline.ToString();
                    tb.Margin = new Thickness(0, newPosition, 0, 0);
                    tb.TextWrapping = TextWrapping.WrapWholeWords;

                    sl.lastPosition = newPosition;

                    headlineBox.Children.Add(tb);
                }

                scrollPostion += 1;

                 await Task.Delay(TimeSpan.FromMilliseconds(35));
            }
        }


        private async Task<Headline[]> getHeadlines()
        {
            try {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(await httpClient.GetStringAsync(rssUri));
                String xmlString = xmlDocument.ToString();
                return parseHeadlinesFromXml(xmlDocument).ToArray();
            }
            catch
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

            return headlines;
        }
    }
}
