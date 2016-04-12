using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spiegel
{
    class Headline
    {
        public string title{ get; private set; }
        public string subtitle { get; private set; }
        public DateTime date { get; private set; }

        public Headline(string title, string subtitle, DateTime date)
        {
            this.title = title;
            this.subtitle = subtitle;
            this.date = date;
        }
        public override String ToString()
        {
            return title ;
        }

    }
}
