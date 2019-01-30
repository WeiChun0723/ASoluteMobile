using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ASolute_Mobile
{
   public  class CustomNavigationPage : NavigationPage
    {
        public CustomNavigationPage(Page rootPage) : base(rootPage)
        {
            BarBackgroundColor = Color.FromHex("#9A2116");
            

        }
    }
}
