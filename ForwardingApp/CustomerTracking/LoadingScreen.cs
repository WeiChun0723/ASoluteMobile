using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public class LoadingScreen : ContentPage
    {
        readonly Image splashImage, splashDeviceImage;

        readonly StackLayout layout;
        ScrollView ScrollView;

        public LoadingScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);

            var sub = new AbsoluteLayout();

            splashImage = new Image
            {
                Source = "appIcon.png",
                HeightRequest = 100,
                WidthRequest = 100
            };

            splashDeviceImage = new Image
            {
                Source = "splashScreenlogo.png",
                HeightRequest = 300,
                WidthRequest = 150,
                IsVisible = false
            };


            Label welcome = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Loading.......",
                FontAttributes = FontAttributes.Bold,
                FontSize = 20,

            };

            AbsoluteLayout.SetLayoutFlags(splashImage, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(splashImage, new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            sub.Children.Add(splashImage);


            layout = new StackLayout
            {
                Spacing = 20,
                Padding = new Thickness(15, 10),
                //HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            ScrollView = new ScrollView();

            StackLayout gap = new StackLayout
            {
                Spacing = 20,
                Padding = new Thickness(15, 15),
                VerticalOptions = LayoutOptions.Center
            };

            layout.Children.Add(sub);
            layout.Children.Add(splashDeviceImage);
            layout.Children.Add(welcome);
       

            ScrollView.Content = layout;
            this.BackgroundColor = Color.FromHex("#ffffff");
            this.Content = ScrollView;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            uint duration = 6 * 1000;

            await Task.WhenAll(

              splashImage.RotateYTo(13 * 360, duration)
            );


            Application.Current.MainPage = new MainPage();
        }
    }
}
