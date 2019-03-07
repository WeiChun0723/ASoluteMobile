using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ASolute_Mobile
{
   public  class CustomNavigationPage : NavigationPage
    {
        public static Label title1, title2;

        public CustomNavigationPage(Page rootPage) : base(rootPage)
        {
            BarBackgroundColor = Color.FromHex("#9A2116");

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
        }
    }
}
