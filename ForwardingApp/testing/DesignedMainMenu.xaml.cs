using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.testing
{
    public partial class DesignedMainMenu : ContentPage
    {
        List<clsKeyValue> checkItems = new List<clsKeyValue>();

        public DesignedMainMenu()
        {
            InitializeComponent();

            GetData();
        }

        async void GetData()
        {
            try
            {
                var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadMenuURL());
                clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (login_response.IsGood == true)
                {
                    var login_Menu = JObject.Parse(content)["Result"].ToObject<clsLogin>();

                    //load value from the menu in json response "CheckList"
                    for (int check = 0; check < login_response.Result["Checklist"].Count; check++)
                    {
                        string itemKey = login_response.Result["Checklist"][check]["Key"];
                        string itemValue = login_response.Result["Checklist"][check]["Value"];

                        checkItems.Add(new clsKeyValue(itemKey, itemValue));
                    }

                    // clear the db before insert to it to prevent duplicate
                    App.Database.deleteMainMenu();
                    App.Database.deleteMenuItems("MainMenu");

                    foreach (clsDataRow mainMenu in login_Menu.MainMenu)
                    {
                        ListItems existingRecord = App.Database.GetMenuRecordAsync(mainMenu.Id);

                        if (mainMenu.Id != "LogOff")
                        {
                            if (existingRecord == null || (!(existingRecord != null)))
                            {
                                if (existingRecord == null)
                                {
                                    existingRecord = new ListItems();
                                }

                                existingRecord.menuId = mainMenu.Id;
                                existingRecord.name = mainMenu.Caption;
                                existingRecord.action = mainMenu.Action;
                                //existingRecord.image = "Image1.png";

                                List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(mainMenu.Id, "MainMenu");

                                int index = 0;
                                foreach (clsCaptionValue summaryList in mainMenu.Summary)
                                {
                                    SummaryItems summaryItem = null;
                                    if (index < existingSummaryItems.Capacity)
                                    {
                                        summaryItem = existingSummaryItems.ElementAt(index);
                                    }

                                    if (summaryItem == null)
                                    {
                                        summaryItem = new SummaryItems();
                                    }

                                    if (String.IsNullOrEmpty(summaryList.Caption))
                                    {
                                        existingRecord.name = summaryList.Value;
                                    }

                                    summaryItem.Id = mainMenu.Id;
                                    summaryItem.Caption = summaryList.Caption;
                                    summaryItem.Value = summaryList.Value;
                                    summaryItem.Display = summaryList.Display;
                                    summaryItem.Type = "MainMenu";
                                    summaryItem.BackColor = mainMenu.BackColor;
                                    App.Database.SaveSummarysAsync(summaryItem);
                                    index++;
                                }

                                App.Database.SaveMenuAsync(existingRecord);
                                if (existingSummaryItems != null)
                                {
                                    for (; index < existingSummaryItems.Count; index++)
                                    {
                                        App.Database.DeleteSummaryItem(existingSummaryItems.ElementAt(index));
                                    }
                                }
                            }
                        }
                    }

                    foreach (clsDataRow contextMenu in login_Menu.ContextMenu)
                    {
                        List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(contextMenu.Id, "ContextMenu");

                        int index = 0;
                        foreach (clsCaptionValue summaryList in contextMenu.Summary)
                        {
                            SummaryItems summaryItem = null;
                            if (index < existingSummaryItems.Capacity)
                            {
                                summaryItem = existingSummaryItems.ElementAt(index);
                            }

                            if (summaryItem == null)
                            {
                                summaryItem = new SummaryItems();
                            }

                            summaryItem.Id = contextMenu.Id;
                            summaryItem.Caption = summaryList.Caption;
                            summaryItem.Value = summaryList.Value;
                            summaryItem.Display = summaryList.Display;
                            summaryItem.Type = "ContextMenu";
                            App.Database.SaveSummarysAsync(summaryItem);
                            index++;
                        }

                        if (existingSummaryItems != null)
                        {
                            for (; index < existingSummaryItems.Count; index++)
                            {
                                App.Database.DeleteSummaryItem(existingSummaryItems.ElementAt(index));
                            }
                        }
                    }

                    LoadMainMenu();
                    //loadDashBoard();
                    // if the check list not empty then display the check list page
                    if (checkItems.Count != 0)
                    {
                        CheckList chkList = new CheckList(checkItems, login_Menu.CheckListLinkId);
                        NavigationPage.SetHasBackButton(chkList, false);
                        await Navigation.PushAsync(chkList);
                    }
                }
                else
                {
                    if (login_response.Message == "Invalid Session !")
                    {
                        BackgroundTask.Logout(this);
                        await DisplayAlert("Error", login_response.Message, "Ok");
                    }
                    else
                    {
                        await DisplayAlert("Error", login_response.Message, "Ok");
                    }

                }

            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Unable to connect", "Please try again later", "Ok");
            }
            catch (Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "Ok");
            }
        }


        // load the item that stored in db to the list view by using custom view cell
        public void LoadMainMenu()
        {
  
            Ultis.Settings.List = "Main_Menu";
            ObservableCollection<ListItems> Item = new ObservableCollection<ListItems>(App.Database.GetMainMenuItems());
            listView.ItemsSource = Item;
            listView.Style = (Style)App.Current.Resources["recordListStyle"];
            listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
        }
    }
}
