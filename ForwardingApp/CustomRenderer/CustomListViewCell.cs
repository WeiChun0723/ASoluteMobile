using ASolute.Mobile.Models;
using ASolute_Mobile.CustomerTracking;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using ImageCircle.Forms.Plugin.Abstractions;
using Newtonsoft.Json;
using PCLStorage;
using Syncfusion.XForms.Border;
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
       // public static List<ProviderInfo> providers;
        string color;

        public CustomListViewCell()
        {

        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            try
            {
                if (this.BindingContext != null)
                {

                    if (Ultis.Settings.List == "Main_Menu")
                    {
                        ListItems model = (ListItems)this.BindingContext;
                        summaryRecord = App.Database.GetSummarysAsync(model.Id, "MainMenu");
                    }
                    else
                    {
                        ListItems model = (ListItems)this.BindingContext;
                        summaryRecord = App.Database.GetSummarysAsync(model.Id, Ultis.Settings.List);
                    }

                    StackLayout cellTextWrapper = new StackLayout()
                    {
                        //Padding = new Thickness(10, 10, 10, 10),
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand
                    };

                    bool firstSummaryLine = true;

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
                    }

                    if (Ultis.Settings.List.Equals("ProviderList"))
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

                    View = new Frame
                    {
                        Content = cellTextWrapper,
                        HasShadow = true,
                        Margin = 5

                    };

                    if (!(String.IsNullOrEmpty(color)))
                    {
                        View.BackgroundColor = Color.FromHex(color);
                    }
                }
            }
            catch
            {

            }

        }

        public async void callWebService(string code)
        {
            var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.deleteSavedProviderURL(code),null);
            clsResponse company_response = JsonConvert.DeserializeObject<clsResponse>(content);
        }

    }
}
