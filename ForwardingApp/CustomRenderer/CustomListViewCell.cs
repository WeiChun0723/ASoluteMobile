﻿using ASolute.Mobile.Models;
using ASolute_Mobile.CustomerTracking;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using PCLStorage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Xamarin.Forms;


namespace ASolute_Mobile
{
    public class CustomListViewCell : ViewCell
    {
        public static List<SummaryItems> summaryRecord;
        public static List<ProviderInfo> providers;
        int count = 0;
        string color;

        public CustomListViewCell()
        {

        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (this.BindingContext != null)
            {

                if (Ultis.Settings.List == "Job_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "JobItem");

                }
                else if (Ultis.Settings.List == "HaulageJob_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "HaulageJob");
                }
                else if (Ultis.Settings.List == "Receiving_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "PendingReceiving");
                }
                else if (Ultis.Settings.List == "Loading_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "PendingLoad");
                }
                else if (Ultis.Settings.List == "refuel_List")
                {
                    RefuelHistoryData model = (RefuelHistoryData)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.recordId, "Refuel");
                }
                else if (Ultis.Settings.List == "Main_Menu")
                {
                    ListItems model = (ListItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "MainMenu");
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
                else if (Ultis.Settings.List == "provider_List")
                {
                    ListItems model = (ListItems)this.BindingContext;
                    providers = App.Database.Providers(model.Id);

                }
                else if (Ultis.Settings.List == "container_List")
                {
                    ListItems model = (ListItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "Container");
                }
                else if (Ultis.Settings.List == "category_List")
                {
                    ListItems model = (ListItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "Category");
                }
                else
                {
                    ListItems model = (ListItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, Ultis.Settings.List);
                }


                StackLayout mainLayout = new StackLayout()
                {
                    //Padding = new Thickness(10, 10, 10, 10),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Orientation = StackOrientation.Horizontal
                };

                StackLayout cellTextWrapper = new StackLayout()
                {
                    //Padding = new Thickness(10, 10, 10, 10),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };

                StackLayout cellImageWrapper = new StackLayout()
                {
                    //Padding = new Thickness(10, 10, 10, 10),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };

                bool firstSummaryLine = true;

                if (Ultis.Settings.List == "provider_List")
                {
                    foreach (ProviderInfo items in providers)
                    {
                        Label label = new Label();
                        label.FontAttributes = FontAttributes.Bold;
                        label.Text = items.Name;
                        cellTextWrapper.Children.Add(label);
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

                        cellTextWrapper.Children.Add(label);

                        if (!(String.IsNullOrEmpty(items.BackColor)))
                        {
                            cellTextWrapper.BackgroundColor = Color.FromHex(items.BackColor);
                            color = items.BackColor;
                        }

                        if (items.Id == "Info" && count >= summaryRecord.Count - 1)
                        {
                            cellImageWrapper.Children.Clear();

                            Image userPicture = new Image
                            {
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                VerticalOptions = LayoutOptions.FillAndExpand,
                                HeightRequest = 120,
                                WidthRequest = 100,
                                Source = "user_icon.png"
                            };

                            var image = App.Database.GetUserProfilePicture(Ultis.Settings.SessionUserItem.DriverId);
                            userPicture.Source = (image != null && image.imageData != null) ? ImageSource.FromStream(() => new MemoryStream(image.imageData)) : "user_icon.png";

                            cellImageWrapper.Children.Add(userPicture);
                            count = 0;
                        }

                        count++;
                    }
                }

                // absoluteLayout.Children.Add(cellWrapper);

                if (Ultis.Settings.List.Equals("provider_List"))
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

                            callWebService(menu.Id);
                        }
                    };

                    menuItem.BindingContextChanged += (sender, e) =>
                    {
                        if (((MenuItem)sender).BindingContext != null)
                        {

                        }
                    };

                    this.ContextActions.Add(menuItem);
                }

                mainLayout.Children.Add(cellTextWrapper);
                mainLayout.Children.Add(cellImageWrapper);

                View = new Frame
                {
                    Content = mainLayout,
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
