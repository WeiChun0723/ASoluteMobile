using System;

using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public class PieChart : ContentPage
    {
        public PieChart()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

