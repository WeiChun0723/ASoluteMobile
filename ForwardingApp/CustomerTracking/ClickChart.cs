using System;

using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public class ClickChart : ContentPage
    {
        public ClickChart()
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

