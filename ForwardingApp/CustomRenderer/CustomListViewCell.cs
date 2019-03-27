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
                else if(Ultis.Settings.List == "HaulageJob_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "HaulageJob");
                }
                else if(Ultis.Settings.List == "Receiving_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "PendingReceiving");
                }
                else if(Ultis.Settings.List == "Loading_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "PendingLoad");
                }
                else if (Ultis.Settings.List== "refuel_List")
                {
                    RefuelHistoryData model = (RefuelHistoryData)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.recordId,"Refuel");
                }       
                else if(Ultis.Settings.List == "Main_Menu")
                {
                    ListItems model = (ListItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId,"MainMenu");
                }
                else if (Ultis.Settings.List == "Log_History")
                {
                    Log model = (Log)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.logId, "Log");
                }
                else if (Ultis.Settings.List == "Run_Sheet")
                {
                    JobItems model = (JobItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "HaulageHistory");
                }
                else if (Ultis.Settings.List == "Pending_Collection")
                {
                    ListObject model = (ListObject)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "pendingCollection");
                }
                else if (Ultis.Settings.List == "provider_List")
                {
                    ListItems model = (ListItems)this.BindingContext;
                    providers = App.Database.Providers(model.menuId);  

                }
                else if (Ultis.Settings.List == "container_List")
                {
                    ListItems model = (ListItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId, "Container");
                }
                else if (Ultis.Settings.List == "category_List")
                {
                    ListItems model = (ListItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId, "Category");
                }
                else
                {
                    ListItems model = (ListItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId, Ultis.Settings.List);
                }


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

                        ListItems menu = (ListItems)((MenuItem)sender).BindingContext;
                        int results = 0;
                        results = App.Database.DeleteMenu(menu);
                        if (results > 0)
                        {
                            ListView parent = (ListView)this.Parent;
                            ObservableCollection<ListItems> itemSource = ((ObservableCollection<ListItems>)parent.ItemsSource);
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
            var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.deleteSavedProvider(code));
            clsResponse company_response = JsonConvert.DeserializeObject<clsResponse>(content);
        }

    }
}
