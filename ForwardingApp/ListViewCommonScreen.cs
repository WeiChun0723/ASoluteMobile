using Syncfusion.SfBusyIndicator.XForms;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public class ListViewCommonScreen : ContentPage
    {
        public static Label title1, title2;

        public ListView listView = new ListView();
        public SearchBar search = new SearchBar();
        public Image noDataImage, scanBarCode;
        public SfBusyIndicator loading;
     

        public ListViewCommonScreen()
        {

            StackLayout main = new StackLayout();

            title1 = new Label
            {
                FontSize = 15,
                Text = (Ultis.Settings.Language.Equals("English")) ? "Main Menu" : "Menu Utama",
                TextColor = Color.White
            };

            title2 = new Label
            {
                FontSize = 10,
                Text = Ultis.Settings.SubTitle,
                TextColor = Color.White
            };

            main.Children.Add(title1);
            main.Children.Add(title2);

            NavigationPage.SetTitleView(this, main);

            noDataImage = new Image
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

            mainLayout.Children.Add(loading);
            mainLayout.Children.Add(noDataImage);
            mainLayout.Children.Add(search);
            mainLayout.Children.Add(listView);
            mainLayout.Children.Add(scanBarCode);

            Content = mainLayout;
        }
    }
  
}

