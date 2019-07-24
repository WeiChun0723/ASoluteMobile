using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ASolute_Mobile.Forwarding
{
    public partial class JobListTabbedPage : TabbedPage
    {
        public static bool fromJobDetailPage = false;

        public JobListTabbedPage()
        {
            InitializeComponent();
            this.BarBackgroundColor = Color.FromHex("#9A2116");
            this.Title = "Job List";
           
        }
    }
}
