using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
        public Clock(Grid UiRoot,Config config) : base(UiRoot,"Clock",config, 250, 65, new Thickness(10,10,10,10), HorizontalAlignment.Center, VerticalAlignment.Top, TimeSpan.FromSeconds(1))
        {
            if(!config.hasSetting(name, "clockType"))
            {
                config.makeSetting(name, "clockType", "24");
            }

        }

        public override void update()
        {
            if (config.getEnabled(name))
            {
                //Debug.WriteLine("Weather Forecast: " + Marshal.SizeOf(this));
                setUpdatePeriod(TimeSpan.FromSeconds(1));
                DateTime dateTime = DateTime.Now;
                String minute, second;
                if (dateTime.Minute < 10)
                    minute = "0" + dateTime.Minute;
                else
                    minute = "" + dateTime.Minute;

                if (dateTime.Second < 10)
                    second = "0" + dateTime.Second;
                else
                    second = "" + dateTime.Second;

                clearWidget();

                TextBlock clock = new TextBlock();
                clock.Text = dateTime.Hour % Int32.Parse(config.getSetting(name,"clockType")) + ":" + minute;
                clock.Foreground = new SolidColorBrush(Colors.White);
                clock.TextAlignment = TextAlignment.Center;
                clock.FontSize = 40;
                clock.VerticalAlignment = VerticalAlignment.Top;


                TextBlock seconds = new TextBlock();
                seconds.FontSize = 20;
                seconds.Foreground = new SolidColorBrush(Colors.White);
                seconds.Text = second;
                seconds.Margin = new Thickness(180, 10, 0, 0);


                TextBlock date = new TextBlock();
                date.Text = dateTime.DayOfWeek + " " + dateTime.Day.ToString() + " " + dateTime.ToString("MMMM") + "," + dateTime.Year;
                date.Foreground = new SolidColorBrush(Colors.White);
                date.TextAlignment = TextAlignment.Center;
                date.FontSize = 20;
                date.VerticalAlignment = VerticalAlignment.Bottom;

                addToWidget(seconds);
                addToWidget(clock);
                addToWidget(date);
            }
            else
            {
                setUpdatePeriod(TimeSpan.FromMilliseconds(100));
                clearWidget();
            }
        }
    }
}
