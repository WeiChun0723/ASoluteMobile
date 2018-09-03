using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace ASolute_Mobile.Courier
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OrderEntry : ContentPage
	{
		public OrderEntry ()
		{
			InitializeComponent ();
            navigationButton.IsVisible = true;
        }

        public async void ToNextPage(object s, EventArgs e)
        {
            if (sender.IsVisible)
            {
                sender.IsVisible = false;
                recipient.IsVisible = true;
                
            }
            else if (recipient.IsVisible)
            {
                senderInfo.IsVisible = true;
                RecInfo.IsVisible = true;
                PackageInfo.IsVisible = true;

            }
        }


        public async void TakeImage(object sender, EventArgs e)
        {

        }

        public async void ToTCPage(object sender, EventArgs e)
        {

        }

        public async void BackToPrevious(object sender, EventArgs e)
        {

        }
    }
}