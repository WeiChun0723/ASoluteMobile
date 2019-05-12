using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ASolute_Mobile.ZebraPrinter
{
    public partial class test : ContentPage
    {
        public test()
        {
            InitializeComponent();
        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            NavigationPage.PushAsync(new ZebraPrinterList());
        }
    }
}
