using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CheckList : ContentPage
    {
        string chkid;
        
        List<clsKeyValue> checkingList = new List<clsKeyValue>();

        public CheckList (List<clsKeyValue> items,string id)
		{
			InitializeComponent ();

            Title = "Check list";
            chkid = id;

            //checkList.RowHeight = 100;
            BindingContext = new CheckListViewModel(items);
        }
        
        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                string answer = "";
                if (Ultis.Settings.Language.Equals("English"))
                {
                    answer = "Are you sure you want to close this screen without submitting the data?";
                }
                else
                {
                    answer = "Sahkan untuk membuang perubahan?";
                }

                if (await DisplayAlert("", answer, "Yes", "No"))
                {
                    base.OnBackButtonPressed();
                    checkingList.Clear();

                    string uri = ControllerUtil.postCheckList(false, "",0);

                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                    var content = JsonConvert.SerializeObject(checkingList);
                    var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(uri, httpContent);
                    var reply = await response.Content.ReadAsStringAsync();

                    Debug.WriteLine(reply);
                    clsResponse result = JsonConvert.DeserializeObject<clsResponse>(reply);

                    if (result.IsGood)
                    {
                        await DisplayAlert("Success", "Check list suspended", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Json Error", result.Message, "OK");
                    }

                    await Navigation.PopToRootAsync();
                }
            });

            return true;
        }
          
        public void disableList(object sender, ToggledEventArgs e)
        {
            if (selectAll.Checked == true)
            {
                checkList.IsEnabled = false;
                checkList.Opacity = 0.5;
            }
            else if (selectAll.Checked == false)
            {
                checkList.IsEnabled = true;
                checkList.Opacity = 1.0;
            }
         }

        public async void toNextPage(object sender, EventArgs e)
        {
            checkingList.Clear();
            try
            {
                if (selectAll.Checked == true)
                {
                    checkingList.Clear();
                }
                else
                {
                    ObservableCollection<CheckListItem> checkedItems = new ObservableCollection<CheckListItem>();
                    checkedItems = CheckListViewModel.CheckedList;
                    foreach (CheckListItem item in checkedItems)
                    {
                        if (item.IsSelected == false)
                        {
                            checkingList.Add(new clsKeyValue(item.Category, item.Name));

                        }

                    }
                }

                CheckList2 step2 = new CheckList2(checkingList,chkid);
                step2.previousPage = this;
                await Navigation.PushAsync(step2);
            }
            catch(Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "Ok");
            }
        }

    }
}