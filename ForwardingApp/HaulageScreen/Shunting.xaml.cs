using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.HaulageScreen
{

	public partial class Shunting : ContentPage
	{
        List<string> containerNumber = new List<string>();
        ObservableCollection<string> number = new ObservableCollection<string>();

        public Shunting (string title)
		{
			InitializeComponent ();

            StackLayout main = new StackLayout();

            Label title1 = new Label
            {
                FontSize = 15,
                Text = title,
                TextColor = Color.White
            };

            Label title2 = new Label
            {
                FontSize = 10,
                Text = Ultis.Settings.SubTitle,
                TextColor = Color.White
            };

            main.Children.Add(title1);
            main.Children.Add(title2);

            NavigationPage.SetTitleView(this, main);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Ultis.Settings.NewJob.Equals("Yes"))
            {
                CommonFunction.CreateToolBarItem(this);
            }
            else
            {
                this.ToolbarItems.Clear();
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing", (sender) => {

                try
                {
                    CommonFunction.NewJobNotification(this);
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        public async void Confirm(object sender, EventArgs e)
        {
            clsCommonFunc checker = new clsCommonFunc();
            //string containerNum = contPrefix.Text + contNumber.Text;
            if(!(String.IsNullOrEmpty(contPrefix.Text)) && !(String.IsNullOrEmpty(contNumber.Text)))
            {
                bool status = checker.CheckContainerDigit(contPrefix.Text + contNumber.Text);

                if (status == true)
                {
                     CallWebService();
                }
                else if (status == false)
                {
                    var answer = await DisplayAlert("Error", "Invalid container check digit, continue?", "Yes", "No");
                    if (answer.Equals(true))
                    {
                        CallWebService();
                    }
                    else
                    {
                        contNumber.Text = String.Empty;
                        contPrefix.Text = String.Empty;
                    }

                }
            }
            else
            {
                await DisplayAlert("Error", "Please fill in container prefix and container number", "OK");
            }
           
           
        }

        public async void CallWebService()
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                var uri = ControllerUtil.postContainerNumberURL(contPrefix.Text + contNumber.Text);
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);

                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (json_response.IsGood == true)
                {

                    await DisplayAlert("Success", "Data uploaded", "OK");
                    
                    containerNumber.Add(contPrefix.Text + contNumber.Text);
                    number.Add(contPrefix.Text + contNumber.Text);
                    containerNumber.Count();
                    string test = contPrefix.Text + contNumber.Text;
                    
                    shuntingHistory.ItemsSource = number;
                }
                else
                {                  
                    await DisplayAlert("Error", json_response.Message, "OK");
                }

            
            }
            catch (Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "OK");
            }
        }


        public void ContainerPrefix(object sender, TextChangedEventArgs e)
        {
            string _prefixtext = contPrefix.Text.ToUpper();
            contPrefix.Text = _prefixtext;

            if(contPrefix.Text.Count() == 4)
            {
                contNumber.Focus();
            }
        }

        public void ContainerNumber(object sender, TextChangedEventArgs e)
        {
            if(contNumber.Text.Count() == 7)
            {
                contNumber.Unfocus();
            }
        }

    }
}