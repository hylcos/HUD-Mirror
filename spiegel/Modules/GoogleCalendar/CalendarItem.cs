using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spiegel
{
    class CalendarItem
    {
        public String description { get; private set; }
        public String location { get; private set; }
        public DateTime startDate { get; private set; }
        public DateTime endDate { get; private set; }
        public int lineNumbers { get; private set; }

        public CalendarItem(String description,DateTime startDate, DateTime endDate,String location)
        {
            lineNumbers = Math.Abs(description.Length / 59) + 4;
            this.description = description;
            this.startDate = startDate;
            this.endDate = endDate;
            this.location = location;
        }
        public DateTime getStartDate()
        {
            return startDate;
        }
        public String getDescription()
        {
            return description;
        }
        public override String ToString()
        {
            return description + "\n" + 
                startDate.ToString("H:mm:ss MM/dd") + "\t-\t" + 
                endDate.ToString("H:mm:ss MM/dd") + "\n" + 
                location;
        }
    }
}
