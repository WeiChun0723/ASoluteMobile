using System;

using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public class CustomerRegistration : ContentPage
    {
        public CustomerRegistration()
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

