using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASolute_Mobile.Models.HaulageViewModel;
using Xamarin.Forms;

namespace ASolute_Mobile.HaulageScreen
{
    public partial class SplashScreen_ : ContentPage
    {

        public SplashScreen_()
        {   
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

            BindingContext = new GetBaseURLViewModel();
          
        }

      
    }
}
