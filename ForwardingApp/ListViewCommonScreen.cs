using Syncfusion.SfBusyIndicator.XForms;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public class ListViewCommonScreen : ContentPage
    {
        public static Label title1, title2;

        public ListView listView = new ListView();
        public Image image, scanBarCode;
        public SfBusyIndicator loading;
     

        public ListViewCommonScreen()
        {

            StackLayout main = new StackLayout();

            title1 = new Label
            {
                FontSize = 15,
                //Text = (Ultis.Settings.Language.Equals("English")) ? "Main Menu" : "Menu Utama",
                TextColor = Color.White
            };

            title2 = new Label
            {
                FontSize = 10,

                TextColor = Color.White
            };

            main.Children.Add(title1);
            main.Children.Add(title2);

            NavigationPage.SetTitleView(this, main);

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

            loading = new SfBusyIndicator
            {
                IsVisible = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                AnimationType = AnimationTypes.HorizontalPulsingBox,
                TextColor = Color.Red
            };

            StackLayout mainLayout = new StackLayout
            {
                Padding = new Thickness(15,15,15,15)
            };

            listView.IsPullToRefreshEnabled = true;
            listView.SeparatorColor = Color.White;
            /*StackLayout frameLayout = new StackLayout();
            frameLayout.SetBinding(BackgroundColorProperty, "background");

            label.FontAttributes = FontAttributes.Bold;
            label.SetBinding(Label.TextProperty, "summary");

            frameLayout.Children.Add(label);


            listView.ItemTemplate = new DataTemplate(() =>
            {
                ViewCell viewCell = new ViewCell();

                Frame frame = new Frame
                {
                    Content = frameLayout,
                    HasShadow = true,
                    Margin = 5,
                };

                frame.SetBinding(BackgroundColorProperty, "background");
                viewCell.View = frame;

                return viewCell;
            });*/

            mainLayout.Children.Add(loading);
            mainLayout.Children.Add(image);
            mainLayout.Children.Add(listView);
            mainLayout.Children.Add(scanBarCode);

            Content = mainLayout;
        }
    }
  
}

