using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace spiegel
{
    class Clock : Widget
    {
        public Clock(Grid UiRoot) : base(UiRoot, 100, 65, new Thickness(10,10,10,10), HorizontalAlignment.Center, VerticalAlignment.Top, TimeSpan.FromSeconds(1))
        {
        }

        public override async void update()
        {
            DateTime dateTime = DateTime.Now;

            clearWidget();

            TextBlock clock = new TextBlock();
            //todo leading/padding 0's
            clock.Text = dateTime.Hour + ":" + dateTime.Minute ;
            clock.Foreground = new SolidColorBrush(Colors.White);
            clock.TextAlignment = TextAlignment.Center;
            clock.FontSize = 40;
            clock.VerticalAlignment = VerticalAlignment.Top;

            TextBlock date = new TextBlock();
            date.Text = dateTime.DayOfWeek + " " + dateTime.Day.ToString() + "-" + dateTime.Month.ToString();
            date.Foreground = new SolidColorBrush(Colors.White);
            date.TextAlignment = TextAlignment.Center;
            date.FontSize = 14;
            date.VerticalAlignment = VerticalAlignment.Bottom;


            addToWidget(clock);
            addToWidget(date);
        }
    }
}
