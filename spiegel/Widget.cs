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
    class Widget: Updateable
    {
        private const bool showWidgetOutline = true;
        protected Grid widgetBox { get; set; }

        public Widget(Grid UiRoot, int width, int height, Thickness margin, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, TimeSpan updatePeriod) : base(updatePeriod)
        {
            widgetBox = new Grid();
            widgetBox.Width = width;
            widgetBox.Height = height;
            widgetBox.Margin = margin;
            widgetBox.HorizontalAlignment = horizontalAlignment;
            widgetBox.VerticalAlignment = verticalAlignment;

            if (showWidgetOutline)
            {
                widgetBox.BorderBrush = new SolidColorBrush(Colors.White);
                widgetBox.BorderThickness = new Thickness(1);
            }

            UiRoot.Children.Add(widgetBox);
        }

        protected void clearWidget()
        {
            widgetBox.Children.Clear();
        }

        protected void addToWidget(UIElement element)
        {
            widgetBox.Children.Add(element);
        }
    }
}
