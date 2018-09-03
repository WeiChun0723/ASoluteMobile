using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.QrCode;

namespace ASolute_Mobile.Courier
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Registration : ContentPage
	{
        string accountType;
		public Registration (string account)
		{
			InitializeComponent ();
            accountType = account;
            if (account.Equals("Business"))
            {
                Title = "Step 1 of 3(Business)";
                Business.IsVisible = true;
            }
            else
            {
                Title = "Step 1 of 3(Personal)";
                Personal.IsVisible = true;
            }

            lblCity.Text = "City & State";

            try
            {
                barCode.BarcodeValue = "1234";
                barCode.BarcodeOptions = new QrCodeEncodingOptions {
                    Height = 150,
                    Width = 250
                };

            }
            catch (Exception exception)
            {
                DisplayAlert("Test", exception.Message, "OK");
            }
        }

        protected override void OnAppearing()
        {
           
            
        }

        public void BusinessInfo()
        {
           
                lblContact.IsVisible = true;
                contactPerson.IsVisible = true;
                lblOffice.IsVisible = true;
                officeTel.IsVisible = true;
                        
        }

        public async void ToNextPage(object sender, EventArgs e)
        {
            if (accountType.Equals("Business"))
            {
                if (Business.IsVisible && AddrInfo.IsVisible)
                {
                    await DisplayAlert("OK", "sended", "ok");
                }
                else if (Business.IsVisible)
                {              
                    Title = "Step 2 of 3(Business)";
                    AddrInfo.IsVisible = true;
                    BusinessInfo();
                    Business.IsVisible = false;
                }
                else if(AddrInfo.IsVisible)
                {
                    AddrInfo.IsVisible = true;
                    BusinessInfo();
                    Business.IsVisible = true;
                }
                
            }
            else
            {

                if (Personal.IsVisible && AddrInfo.IsVisible)
                {
                    await DisplayAlert("OK", "sended", "ok");
                }
                else if (Personal.IsVisible)
                {                  
                    Title = "Step 2 of 3(Personal)";
                    AddrInfo.IsVisible = true;
                    Personal.IsVisible = false;
                }
                else if (AddrInfo.IsVisible)
                {
                    AddrInfo.IsVisible = true;
                    Personal.IsVisible = true;
                    term.IsVisible = true;
                    
                }
                
            }
        }

        public async void BackToPrevious(object sender, EventArgs e)
        {
            if (accountType.Equals("Business"))
            {
                if (AddrInfo.IsVisible)
                {
                    
                    Title = "Step 1 of 3(Business)";
                    AddrInfo.IsVisible = false;
                    Business.IsVisible = true;
                }
                else
                {
                    await Navigation.PushAsync(new LoginPage());
                }
            }
            else
            {
                if (AddrInfo.IsVisible)
                {                
                    Title = "Step 1 of 3(Personal)";
                    AddrInfo.IsVisible = false;
                    Personal.IsVisible = true;
                }
                else
                {
                    await Navigation.PushAsync(new LoginPage());
                }
            }
        }

        public async void ToTCPage(object sender, EventArgs e)
        {
            
        }

        public async void TakeImage(object sender, EventArgs e)
        {

        }
    }
}