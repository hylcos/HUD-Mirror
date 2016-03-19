using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace spiegel
{
    class GCal
    {
        private const String protocol   = "https";
        private const String host       = "www.googleapis.com";
        private const int port = 443;
        private String apiKey;
        private HttpClient httpClient;
        public GCal(String apiKey)
        {
            httpClient = new HttpClient();
            this.apiKey = apiKey;
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
                new WebUrl.Query("key", apiKey)
            };
            webUrl.addPath(paths);
            webUrl.addQuery(querys);
            Uri url = webUrl.compose();
            JsonObject json;
            try
            {
                //json = JsonObject.Parse(await httpClient.GetStringAsync(url));
                //JsonArray jsonArray = json.GetNamedArray("items");
                String[] items = {
                    "hylcos@gmail.com",
                    "n4nh2en8nl0tt5lsq2mfiobgms@group.calendar.google.com ",
                    "roc1vp9b0qd7rea20s44alh65019lbod@import.calendar.google.com",
                    "#contacts@group.v.calendar.google.com",
                    "en.dutch#holiday@group.v.calendar.google.com"
                };
                foreach (String item in items)
                {
                    //JsonObject it = item.GetObject();
                    //String id = it.GetNamedString("id");
                    CalendarItem[] tmpCalendarItems = await getItems(item);
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

    }
}
