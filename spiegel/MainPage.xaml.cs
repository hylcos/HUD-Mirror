﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace spiegel
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Nos nosFeed;
       


        private int i = 0;
        public MainPage()
        {
            this.InitializeComponent();
            nosFeed = new Nos();
            updateTempThread();
        }

        private async void updateTempThread()
        {
            
            try {
                Headline[] nosHeadlines = await nosFeed.getHeadlines();
                temperature.Text = nosHeadlines.First().title;
            }
            catch(UnableToParseFeedException e)
            {

            }
        }
    }
}
