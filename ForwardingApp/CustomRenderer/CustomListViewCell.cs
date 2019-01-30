using ASolute.Mobile.Models;
using ASolute_Mobile.CustomerTracking;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;


namespace ASolute_Mobile
{
    public class CustomListViewCell : ViewCell
    {
        public static List<SummaryItems> summaryRecord ;
        public static List<ProviderInfo> providers;

        string color;
        
        public CustomListViewCell()
        {
           
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if(this.BindingContext != null)
            {
                
                if (Ultis.Settings.List == "Job_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "JobItem");

                }
                if(Ultis.Settings.List == "Job_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "HaulageJob");
                }
                if(Ultis.Settings.List == "Receiving_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "PendingReceiving");
                }
                if(Ultis.Settings.List == "Loading_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "PendingLoad");
                }
                if (Ultis.Settings.List== "refuel_List")
                {
                    RefuelHistoryData model = (RefuelHistoryData)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.recordId,"Refuel");
                }       
                if(Ultis.Settings.List == "Main_Menu")
                {
                    AppMenu model = (AppMenu)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId,"MainMenu");
                }
                if (Ultis.Settings.List == "Log_History")
                {
                    Log model = (Log)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.logId, "Log");
                }
                if (Ultis.Settings.List == "Run_Sheet")
                {
                    JobItems model = (JobItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "HaulageHistory");
                }
                if (Ultis.Settings.List == "Pending_Collection")
                {
                    ListObject model = (ListObject)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "pendingCollection");
                }
                if (Ultis.Settings.List == "provider_List")
                {
                    AppMenu model = (AppMenu)this.BindingContext;
                    providers = App.Database.Providers(model.menuId);  

                }
                if (Ultis.Settings.List == "container_List")
                {
                    AppMenu model = (AppMenu)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId, "Container");
                }
                if (Ultis.Settings.List == "category_List")
                {
                    AppMenu model = (AppMenu)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId, "Category");
                }
                if (Ultis.Settings.List == "Picking_List")
                {
                    AppMenu model = (AppMenu)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId, "PickingList");
                }
                if (Ultis.Settings.List== "Packing_List")
                {
                    AppMenu model = (AppMenu)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId, "PackingList");
                }
                if (Ultis.Settings.List == "TallyOut_List")
                {
                    AppMenu model = (AppMenu)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId, "TallyOutList");
                }
                if (Ultis.Settings.List == "TallyIn_List")
                {
                    AppMenu model = (AppMenu)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId, "TallyInList");
                }

                //  AbsoluteLayout absoluteLayout = new AbsoluteLayout();

                StackLayout cellWrapper = new StackLayout()
                {
                    //Padding = new Thickness(10, 10, 10, 10),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };

                bool firstSummaryLine = true;

                if(Ultis.Settings.List == "provider_List" )
                {
                    foreach (ProviderInfo items in providers)
                    {
                        Label label = new Label();
                        label.FontAttributes = FontAttributes.Bold;
                        label.Text = items.Name;
                        cellWrapper.Children.Add(label);
                    }

                }
                else
                {

                    foreach (SummaryItems items in summaryRecord)
                    {
                        Label label = new Label();
                        label.FontSize = 15;

                        if (items.Caption == "" || items.Caption == "Job No." || items.Caption == "Consignee" || Ultis.Settings.List.Equals("category_List"))
                        {
                            label.Text = items.Value;

                        }
                        else if (items.Caption == "Action" || items.Display == false)
                        {
                            label.IsVisible = false;
                        }
                        else
                        {
                            label.Text = items.Caption + ": " + items.Value;
                        }

                        if (firstSummaryLine)
                        {

                            firstSummaryLine = false;
                            label.FontAttributes = FontAttributes.Bold;
                        }


                        cellWrapper.Children.Add(label);

                        if (!(String.IsNullOrEmpty(items.BackColor)))
                        {
                            cellWrapper.BackgroundColor = Color.FromHex(items.BackColor);
                            color = items.BackColor;
                        }

                    }
                }
                       
               // absoluteLayout.Children.Add(cellWrapper);

                if(Ultis.Settings.List.Equals("provider_List"))
                {
                    MenuItem menuItem = new MenuItem()
                    {
                        Text = "Delete",
                        IsDestructive = true
                    };

                    menuItem.Clicked += (sender, e) =>
                    {

                        AppMenu menu = (AppMenu)((MenuItem)sender).BindingContext;
                        int results = 0;
                        results = App.Database.DeleteMenu(menu);
                        if (results > 0)
                        {
                            ListView parent = (ListView)this.Parent;
                            ObservableCollection<AppMenu> itemSource = ((ObservableCollection<AppMenu>)parent.ItemsSource);
                            if (itemSource.Contains(menu))
                            {
                                itemSource.Remove(menu);
                            }

                            callWebService(menu.menuId);
                        }
                    };

                    menuItem.BindingContextChanged += (sender, e) => {
                        if (((MenuItem)sender).BindingContext != null)
                        {

                        }
                    };

                    this.ContextActions.Add(menuItem);
                }
 
                 View = new Frame
                 {
                    Content = cellWrapper,
                    HasShadow = true,
                    Margin = 5
                 
                 };

                if (!(String.IsNullOrEmpty(color)))
                {
                    View.BackgroundColor = Color.FromHex(color);
                }
                
            }

        }

        public async void callWebService(string code)
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.deleteSavedProvider(code));
            clsResponse company_response = JsonConvert.DeserializeObject<clsResponse>(content);
        }

    }
}
