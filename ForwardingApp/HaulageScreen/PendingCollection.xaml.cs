using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PendingCollection : ContentPage
	{
       
		public PendingCollection (string title)
		{
			InitializeComponent ();
            if(NetworkCheck.IsInternet())
            {
                downloadPendingCollection();
            }
            else
            {
                refreshPendingCollection();
            }
            
        
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


        protected void pendingCollectionRefresh(object sender, EventArgs e)
        {
            downloadPendingCollection();
            pendingCollection.IsRefreshing = false;
        }

        public void refreshPendingCollection()
        {
            Ultis.Settings.List = "Pending_Collection";
            ObservableCollection<ListObject> Item = new ObservableCollection<ListObject>(App.Database.GetPendingObject());
            pendingCollection.ItemsSource = Item;
            pendingCollection.HasUnevenRows = true;
            pendingCollection.Style = (Style)App.Current.Resources["recordListStyle"];
            pendingCollection.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            if (Item.Count == 0)
            {
                pendingCollection.IsVisible = false;
                noData.IsVisible = true;
            }
            else
            {
                pendingCollection.IsVisible = true;
                noData.IsVisible = false;
            }
        }

        public async void downloadPendingCollection()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
            var uri = ControllerUtil.getPendingCollectionURL();
            var response = await client.GetAsync(uri);
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(content);

            clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (json_response.IsGood == true)
            {
                var Collections = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

                App.Database.deletePendingObject();
                App.Database.deleteHaulageSummary("pendingCollection");

                foreach (clsDataRow data in Collections)
                {
                    Guid objectID = Guid.NewGuid();
                    ListObject existingRecord = new ListObject();
                    existingRecord.Id = objectID.ToString();
                    existingRecord.type = "pendingCollection";
                    App.Database.SavePendingAsync(existingRecord);

                    List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync("", "pendingCollection");

                    int index = 0;
                    foreach (clsCaptionValue summaryList in data.Summary)
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

                        summaryItem.Id = objectID.ToString();
                        summaryItem.Caption = summaryList.Caption;
                        summaryItem.Value = summaryList.Value;
                        summaryItem.Display = summaryList.Display;
                        summaryItem.Type = "pendingCollection";
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

                refreshPendingCollection();
            }
            else
            {
                if (json_response.Message == "Invalid Session !")
                {
                    BackgroundTask.Logout(this);
                    await DisplayAlert("Error", json_response.Message, "Ok");
                }
                else
                {
                    await DisplayAlert("Error", json_response.Message, "Ok");
                }
               
            }
        }
    }
}