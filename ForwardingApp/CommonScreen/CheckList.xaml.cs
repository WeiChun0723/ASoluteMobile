using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Syncfusion.XForms.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Xamarin.Forms;

namespace ASolute_Mobile
{
	public partial class CheckList : ContentPage
    {
        string chkid;
        
        List<clsKeyValue> checkingList = new List<clsKeyValue>();

        public CheckList (List<clsKeyValue> items,string id)
		{
			InitializeComponent ();
            Title = "Check list";
            chkid = id;
            BindingContext = new CheckListViewModel(items);
        }
        
        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                string answer = (Ultis.Settings.Language.Equals("English")) ? "Are you sure you want to close this screen without submitting the data?" : "Sahkan untuk membuang perubahan?";

                if (await DisplayAlert("", answer, "Yes", "No"))
                {
                    base.OnBackButtonPressed();
                    checkingList.Clear();

                    var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.postCheckList(false, "", 0), this);
                    clsResponse result = JsonConvert.DeserializeObject<clsResponse>(content);

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
          
        public void disableList(object sender, StateChangedEventArgs e)
        {
            if (selectAll.IsChecked == true)
            {
                checkList.IsEnabled = false;
                checkList.Opacity = 0.5;
            }
            else if (selectAll.IsChecked == false)
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
                if (selectAll.IsChecked == true)
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
                await Navigation.PushAsync(new CheckList2(checkingList, chkid));

            }
            catch(Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "Ok");
            }
        }

    }
}