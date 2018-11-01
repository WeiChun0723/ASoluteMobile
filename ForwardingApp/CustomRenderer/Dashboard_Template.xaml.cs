using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ASolute_Mobile.CustomRenderer
{
    public partial class Dashboard_Template : ContentView
    {
        public Dashboard_Template()
        {
            InitializeComponent();
        }

        public ImageSource Icon
        {
            get { return DashboardIcon.Source; }
            set { DashboardIcon.Source = value; }
        }

        public string Label
        {
            get { return DashboardLabel.Text; }
            set { DashboardLabel.Text = value; }
        }

    }
}
