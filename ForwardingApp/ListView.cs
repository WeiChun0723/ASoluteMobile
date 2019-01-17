using System;

using Xamarin.Forms;

namespace ASolute_Mobile
{
    public class ListView : ContentPage
    {
        public ListView()
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

