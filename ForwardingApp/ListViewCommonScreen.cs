using System;

using Xamarin.Forms;

namespace ASolute_Mobile
{
    public class ListViewCommonScreen : ContentPage
    {
        public ListView listView = new ListView();
        public Image image, scanBarCode;
        public ActivityIndicator loading ;
        public Label label = new Label();

        public ListViewCommonScreen()
        {
           image = new Image
            {
                Source = "nodatafound.png",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsVisible = false
           };

            scanBarCode = new Image
            {
                Source = "barcode_scanner.png",
                HorizontalOptions = LayoutOptions.End,
                WidthRequest = 70,
                HeightRequest = 70,
                IsVisible = false
            };

            loading = new ActivityIndicator
            {
                IsVisible = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsRunning = true
            };

            StackLayout mainLayout = new StackLayout
            {
                Padding = new Thickness(15,15,15,15)
            };

            StackLayout frameLayout = new StackLayout();

            label.FontAttributes = FontAttributes.Bold;
            label.SetBinding(Label.TextProperty, "Summary");

            frameLayout.Children.Add(label);
            frameLayout.SetBinding(BackgroundColorProperty, "BackColor");

            listView.IsPullToRefreshEnabled = true;
            listView.SeparatorColor = Color.White;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                ViewCell viewCell = new ViewCell();

                Frame frame = new Frame
                {
                    Content = frameLayout,
                    HasShadow = true,
                    Margin = 5,
                };

                frame.SetBinding(BackgroundColorProperty, "BackColor");

                viewCell.View = frame;

                return viewCell;
            });
            
            mainLayout.Children.Add(image);
            mainLayout.Children.Add(listView);
            mainLayout.Children.Add(loading);
            mainLayout.Children.Add(scanBarCode);

            Content = new ScrollView
            {
                Content = mainLayout
            };
        }
    }
  
}

