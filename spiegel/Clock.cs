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
        public Clock(Grid UiRoot) : base(UiRoot, 300, 100, new Thickness(10,10,10,10), HorizontalAlignment.Right, VerticalAlignment.Top, TimeSpan.FromSeconds(1))
        {
        }

        public override async void update()
        {
            DateTime dateTime = DateTime.Now;

            clearWidget();

            Grid grid = new Grid();

            TextBlock clock = new TextBlock();
            //todo leading/padding 0's
            clock.Text = dateTime.DayOfWeek + " " + dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "\n" + dateTime.Hour + " : " + dateTime.Minute + " : " + dateTime.Second ;
            clock.Foreground = new SolidColorBrush(Colors.White);
            clock.TextAlignment = TextAlignment.Right;

            grid.Children.Add(clock);

            addToWidget(grid);
        }
    }
}
