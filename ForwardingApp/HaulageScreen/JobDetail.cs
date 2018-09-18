using System;

using Xamarin.Forms;

namespace ASolute_Mobile.HaulageScreen
{
    public class JobDetail : ContentPage
    {
        public JobDetail()
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

