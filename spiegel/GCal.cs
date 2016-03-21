using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace spiegel
{
    //todo Gcal moet nog erfen van Widget
    class GCal : Widget
    {
        private const String protocol   = "https";
        private const String host       = "www.googleapis.com";
        private const int port = 443;
        private String apiKey;
        private HttpClient httpClient;
        private String access_token="", refresh_token="";
        public GCal(String apiKey, Grid UiRoot, String refresh_token = "") :base(UiRoot,300,600,new Thickness(10,420,10,0),HorizontalAlignment.Left,VerticalAlignment.Top,TimeSpan.FromMinutes(10))
        {
            httpClient = new HttpClient();
            this.apiKey = apiKey;
            this.refresh_token = refresh_token;
            access_token = getAccessToken(refresh_token);
        }
        public override async void update()
        {
            CalendarItem[] calendarItems = await getLatestItems();
            clearWidget();

            Grid grid = new Grid();
            int i = 0;
            foreach (CalendarItem item in calendarItems)
            {
                TextBlock tb = new TextBlock();
                tb.FontSize = 10;
                tb.Foreground = new SolidColorBrush(Colors.White);
                tb.Text = item.ToString();
                tb.Margin = new Thickness(0, tb.FontSize * i, 0, 0);
                tb.TextWrapping = TextWrapping.WrapWholeWords;
                grid.Children.Add(tb);
                i += item.lineNumbers;
            }
            UIElementCollection wow = grid.Children;
            addToWidget(grid);
        }
        public async Task<CalendarItem[] > getLatestItems(int maxItems = 10)
        {
            CalendarItem[] items = await getCalendars();
            List<CalendarItem> SortedList = items.OrderBy(o => o.startDate).ToList().GetRange(0, maxItems);
            return SortedList.ToArray();
        }
        private async Task <CalendarItem[]> getCalendars()
        {
            List<CalendarItem> calendarItems = new List<CalendarItem>();
            WebUrl webUrl = new WebUrl(protocol, host, port);
            string[] paths = { "calendar", "v3", "users", "me", "calendarList" };
            WebUrl.Query[] querys = {
                new WebUrl.Query("key", apiKey),
                new WebUrl.Query("access_token",access_token)
            };
            webUrl.addPath(paths);
            webUrl.addQuery(querys);
            Uri url = webUrl.compose();
            JsonObject json;
            try
            {
                json = JsonObject.Parse(await httpClient.GetStringAsync(url));
                JsonArray jsonArray = json.GetNamedArray("items");
                foreach (JsonValue item in jsonArray)
                {
                    JsonObject it = item.GetObject() ;
                    String id = it.GetNamedString("id");
                    CalendarItem[] tmpCalendarItems = await getItems(id);
                    calendarItems.AddRange(tmpCalendarItems);
                }
            }
            catch (Exception e)
            {

            }
            return calendarItems.ToArray();
        }

        private async Task < CalendarItem[] >getItems(String agendaID)
        {
            List<CalendarItem> calendarItems = new List<CalendarItem>();
            WebUrl webUrl = new WebUrl(protocol, host, port);
            string[] paths = { "calendar", "v3", "calendars", "primary", "events" };
            WebUrl.Query[] querys = {
                new WebUrl.Query("key", apiKey),
                new WebUrl.Query("calendarId", agendaID),
                new WebUrl.Query("timeMin", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                new WebUrl.Query("maxResults","100")
            };
            webUrl.addPath(paths);
            webUrl.addQuery(querys);
            Uri url = webUrl.compose();
            JsonObject json;
            try {
                json = JsonObject.Parse(await httpClient.GetStringAsync(url));
                JsonArray jsonArray = json.GetNamedArray("items");
                foreach (JsonValue item in jsonArray)
                {
                    JsonObject it = item.GetObject();
                    JsonObject start = it.GetNamedObject("start");
                    JsonObject end = it.GetNamedObject("end");
                    DateTime startDate, endDate;
                    try {
                        startDate = Convert.ToDateTime(start.GetNamedString("date"));
                        endDate = Convert.ToDateTime(end.GetNamedString("date"));
                    }catch
                    {
                        startDate = Convert.ToDateTime(start.GetNamedString("dateTime"));
                        endDate = Convert.ToDateTime(end.GetNamedString("dateTime"));
                    }
                    calendarItems.Add(new CalendarItem(it.GetNamedString("summary"),startDate,endDate,null));
                }
            }
            catch(Exception e)
            {

            }
            
            return calendarItems.ToArray();
        }

        private String getAccessToken(String refreshToken = "")
        {
           
                WebUrl webAuthUrl = new WebUrl(protocol, host, port);
                string[] paths = { "oauth2", "v4", "token" };
                webAuthUrl.addPath(paths);
                FormUrlEncodedContent content;
                if (refreshToken == "")
                {
                    content = new FormUrlEncodedContent(new[]
                        {
                    new KeyValuePair<string, string>("code", "4/x68zKiHOoag-uKJBxSGMasGk8c3DLfoC5had_eno-MM"),
                    new KeyValuePair<string, string>("redirect_uri", "http://localhost:8080"),
                    new KeyValuePair<string, string>("client_id", "855714885654-26l03mb5tf08p3agig8n634115tsvueo.apps.googleusercontent.com"),
                    new KeyValuePair<string, string>("client_secret", "LLC0LHxFoszPoLSvqR7xAw7Y"),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                        });
                    }
                    else {
                    content = new FormUrlEncodedContent(new[]
                        {
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("redirect_uri", "http://localhost:8080"),
                    new KeyValuePair<string, string>("client_id", "855714885654-26l03mb5tf08p3agig8n634115tsvueo.apps.googleusercontent.com"),
                    new KeyValuePair<string, string>("client_secret", "LLC0LHxFoszPoLSvqR7xAw7Y"),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                        });
                }
                JsonObject postJson;

                try
                {
                    var result = httpClient.PostAsync(webAuthUrl.compose().ToString(), content).Result;
                    postJson = JsonObject.Parse(result.Content.ReadAsStringAsync().Result);
                    access_token = postJson.GetNamedString("access_token");
                    if (refresh_token == "")
                    {
                        refreshToken = postJson.GetNamedString("refresh_token");
                    }
                }
                catch { }
            
            return access_token;
        }
    }
}
