using System;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ASolute_Mobile.Models.HaulageViewModel
{
    public class GetBaseURLViewModel : PropertyChange
    {
        public Command GetBasedURL { get; }
        bool isBusy = false;
        string enterpriseName = "";

        public GetBaseURLViewModel()
        {
            GetBasedURL = new Command(async () => await GetURL(),
                                            () => !IsBusy);
        }

        public string EnterpriseName
        {
            get
            {
                    return enterpriseName;
               
                 
            }
            set
            {
               
                enterpriseName = value.ToUpper();

                SetProperty(ref enterpriseName, value);
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;

                SetProperty(ref isBusy, value);

                OnPropertyChanged(nameof(IsBusy));
                GetBasedURL.ChangeCanExecute();

            }
        }

        async Task GetURL()
        {
            IsBusy = true;

            if (!(String.IsNullOrWhiteSpace(enterpriseName)))
            {


                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(await CommonFunction.GetWebService("https://api.asolute.com/",
                                                                                       ControllerUtil.getBaseURL(enterpriseName)));
                if (json_response.IsGood)
                {
                    string returnUri = json_response.Result;
                    if (!(returnUri.Contains("Unknown")))
                    {
                        Ultis.Settings.AppEnterpriseName = enterpriseName.ToUpper();
                        Ultis.Settings.SessionBaseURI = json_response.Result + "api/";

                     
                        await Application.Current.MainPage.DisplayAlert("Success", "Enterprise name updated.", "Ok");

                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Enterprise name unknown", "OK");
                    }

                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Json Error", json_response.Message, "OK");
                }

                if(Ultis.Settings.AppFirstInstall.Equals("First"))
                {
                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                    Ultis.Settings.AppFirstInstall = "Second";
                }

            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Missing field", "Please key in enterprise name.", "OK");
            }

            IsBusy = false;
        }
    }
}
