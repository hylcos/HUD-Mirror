﻿using System;
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
        private const bool showWidgetOutline = false;
        protected string name;
        protected Config config;
        protected Grid widgetBox { get; set; }
        protected bool state;

        public Widget(Grid UiRoot,String name, Config config, int width, int height, Thickness margin, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, TimeSpan updatePeriod) : base(updatePeriod)
        {
            this.name = name;
            this.config = config;
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
        public bool updateEnabled()
        {
            bool newState = (config.getSetting(name, "enabled") == "true") ? true : false;
            if(newState != state)
            {
                state = newState;
                return true;
            }
            return false;
        }
        public bool settingChanged()
        {
            try
            {
                if(config.getSetting(name, "changed") == "true" ? true : false)
                {
                    config.setSettingChanged(name, false);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        protected void addToWidget(UIElement element)
        {
            widgetBox.Children.Add(element);
        }
    }
}
