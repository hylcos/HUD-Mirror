using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace spiegel
{
    //todo Gcal moet nog erfen van Widget
    class GCal : Widget
    {
        private const String protocol = "https";
        private const String host = "www.googleapis.com";
        private const int port = 443;
        private String apiKey;
        private HttpClient httpClient;
        private String access_token = "", refresh_token = "";
        private DateTime oldDateTime = DateTime.Now;
        public GCal(String apiKey, Grid UiRoot, Config config) : base(UiRoot, "GoogleCalendar", config, 300, 600, new Thickness(10, 110, 10, 10), HorizontalAlignment.Left, VerticalAlignment.Top, TimeSpan.FromMinutes(10))
        {
            httpClient = new HttpClient();
            this.apiKey = apiKey;
        }
        public async void checkSettings()
        {
            if (!config.hasSetting(name, "auth"))
            {
                await config.makeSetting(name, "auth", "");
            }
            if (!config.hasSetting(name, "access_token"))
            {
                await config.makeSetting(name, "access_token", "");
            }
            if (!config.hasSetting(name, "refresh_token"))
            {
                await config.makeSetting(name, "refresh_token", "");
            }
            if (!config.hasSetting(name, "numberOfDays"))
            {
                await config.makeSetting(name, "numberOfDays", "1");
            }
            if (!config.hasSetting(name, "numberOfPosts"))
            {
                await config.makeSetting(name, "numberOfPosts", "10");
            }
        }
        public override async void update()
        {
            if (state)
            {
                CalendarItem[] calendarItems = await getLatestItems();

                clearWidget();

                Grid grid = new Grid();
                TextBlock tx = new TextBlock();
                tx.Foreground = new SolidColorBrush(Colors.White);
                tx.FontSize = 14;
                tx.Margin = new Thickness(0, 0, 50, 50);
                if (calendarItems != null)
                {
                    CalendarItem[] calenderItemsFiltered = filterItems(calendarItems);
                    tx.FontWeight = FontWeights.Bold;
                    tx.Text = "AGENDA";
                    tx.FontSize = 20;
                    grid.Children.Add(tx);
                    int i = 2;
                    foreach (CalendarItem item in calenderItemsFiltered)
                    {
                        TextBlock tb = new TextBlock();
                        tb.FontWeight = FontWeights.Bold;
                        tb.FontSize = 14;
                        tb.Foreground = new SolidColorBrush(Colors.White);
                        tb.Text = item.getStartDate().ToString("H:mm");
                        tb.Margin = new Thickness(0, tb.FontSize * (i), 0, 0);
                        tb.TextWrapping = TextWrapping.WrapWholeWords;

                        TextBlock name = new TextBlock();
                        name.Text += item.description;
                        name.FontSize = 14;
                        name.Margin = new Thickness(50, name.FontSize * (i), 0, 0);
                        //name.TextWrapping = TextWrapping.WrapWholeWords;
                        name.Foreground = new SolidColorBrush(Colors.White);

                        grid.Children.Add(tb);
                        grid.Children.Add(name);
                        i += 1;
                    }
                    UIElementCollection wow = grid.Children;
                }
                else
                {
                    tx.Text = "You havent logged in!\n You can do this in the android application";
                    grid.Children.Add(tx);
                }
                addToWidget(grid);
            }
            else
            {
                clearWidget();
            }
        }

        private CalendarItem[] filterItems(CalendarItem[] calendarItems)
        {
            int maxDays = Int32.Parse(config.getSetting(name, "numberOfDays"));
            List<CalendarItem> items = new List<CalendarItem>();
            DateTime localDate = DateTime.Now.AddDays(maxDays);
            foreach (CalendarItem calendarItem in calendarItems)
            {
                if (localDate > calendarItem.getStartDate())
                {
                    items.Add(calendarItem);
                }
            }
            return items.ToArray();
        }

        public async Task<CalendarItem[]> getLatestItems()
        {
            access_token = getAccessToken();
            oldDateTime = DateTime.Now;

            CalendarItem[] items = await getCalendars();
            if (items.Length != 0)
            {
                List<CalendarItem> SortedList = items.OrderBy(o => o.startDate).ToList().GetRange(0, Int32.Parse(config.getSetting(name, "numberOfPosts")));
                return SortedList.ToArray();
            }
            return null;
        }
        private async Task<CalendarItem[]> getCalendars()
        {
            using (HttpClient _httpClient = new HttpClient())
            {
                List<CalendarItem> calendarItems = new List<CalendarItem>();
                WebUrl webUrl = new WebUrl(protocol, host, port);
                string[] paths = { "calendar", "v3", "users", "me", "calendarList" };
                WebUrl.Query[] querys = {
                new WebUrl.Query("key", apiKey),
                new WebUrl.Query("access_token",config.getSetting(name, "access_token"))
            };
                webUrl.addPath(paths);
                webUrl.addQuery(querys);
                Uri url = webUrl.compose();
                JsonObject json;
                try
                {
                    json = JsonObject.Parse(await _httpClient.GetStringAsync(url));
                    JsonArray jsonArray = json.GetNamedArray("items");
                    foreach (JsonValue item in jsonArray)
                    {
                        JsonObject it = item.GetObject();
                        String id = it.GetNamedString("id");
                        Debug.WriteLine(id);
                        CalendarItem[] tmpCalendarItems = await getItems(id);
                        calendarItems.AddRange(tmpCalendarItems);
                        Debug.WriteLine(id);
                    }
                }
                catch (Exception e)
                {
                    access_token = getAccessToken(refresh_token);
                }
                return calendarItems.ToArray();
            }
        }

        private async Task<CalendarItem[]> getItems(String agendaID)
        {
            using (HttpClient _httpClient = new HttpClient())
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
                try
                {
                    json = JsonObject.Parse(await _httpClient.GetStringAsync(url));
                    JsonArray jsonArray = json.GetNamedArray("items");
                    foreach (JsonValue item in jsonArray)
                    {
                        JsonObject it = item.GetObject();
                        JsonObject start = it.GetNamedObject("start");
                        JsonObject end = it.GetNamedObject("end");
                        IJsonValue nul = null;
                        DateTime startDate, endDate;
                        if (start.TryGetValue("date", out nul)) {
                            startDate = Convert.ToDateTime(nul);
                            endDate = Convert.ToDateTime(end.GetNamedString("date"));
                        }else { 
                            startDate = Convert.ToDateTime(start.GetNamedString("dateTime"));
                            endDate = Convert.ToDateTime(end.GetNamedString("dateTime"));
                        }
                        calendarItems.Add(new CalendarItem(it.GetNamedString("summary"), startDate, endDate, null));
                    }
                }
                catch (Exception e)
                {
                }
                return calendarItems.ToArray();
            }
        }

        private String getAccessToken(String refreshToken = "")
        {
            using (HttpClient _httpClient = new HttpClient())
            {
                WebUrl webAuthUrl = new WebUrl(protocol, host, port);
                string[] paths = { "oauth2", "v4", "token" };
                webAuthUrl.addPath(paths);
                FormUrlEncodedContent content;
                if (config.getSetting(name, "refresh_token") == "")
                {
                    content = new FormUrlEncodedContent(new[]
                        {
                    new KeyValuePair<string, string>("code", config.getSetting(name,"auth")),
                    new KeyValuePair<string, string>("redirect_uri", "http://hylcos.com"),
                    new KeyValuePair<string, string>("client_id", "855714885654-h2ojbbar2453n1g0n5j3f0kp9g4pmhll.apps.googleusercontent.com"),
                    new KeyValuePair<string, string>("client_secret", "nyBiwLPPCGTL6HoJhQgY4RLs"),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                        });
                }
                else
                {
                    content = new FormUrlEncodedContent(new[]
                        {
                    new KeyValuePair<string, string>("refresh_token", config.getSetting(name,"refresh_token")),
                    new KeyValuePair<string, string>("redirect_uri", "http://hylcos.com"),
                    new KeyValuePair<string, string>("client_id", "855714885654-h2ojbbar2453n1g0n5j3f0kp9g4pmhll.apps.googleusercontent.com"),
                    new KeyValuePair<string, string>("client_secret", "nyBiwLPPCGTL6HoJhQgY4RLs"),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                        });
                }
                JsonObject postJson;

                try
                {
                    var result = _httpClient.PostAsync(webAuthUrl.compose().ToString(), content).Result;
                    postJson = JsonObject.Parse(result.Content.ReadAsStringAsync().Result);
                    config.setSetting(name, "access_token", postJson.GetNamedString("access_token"));
                    Debug.WriteLine("Refresh token");
                    refreshToken = postJson.GetNamedString("refresh_token");
                    config.setSetting(name, "refresh_token", refreshToken);
                }
                catch
                {
                    Debug.WriteLine("GCAL: Something Happend !!!!");
                }

                return access_token;
            }
        }
    }
}
