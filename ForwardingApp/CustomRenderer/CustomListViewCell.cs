using ASolute_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;


namespace ASolute_Mobile
{
    public class CustomListViewCell : ViewCell
    {
        public static List<SummaryItems> summaryRecord;
        string color;
        
        public CustomListViewCell()
        {
           
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if(this.BindingContext != null)
            {
                
                if (Ultis.Settings.ListType == "Job_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "JobItem");

                }
                if(Ultis.Settings.ListType == "Job_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "HaulageJob");
                }
                if(Ultis.Settings.ListType == "Receiving_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "PendingReceiving");
                }
                if(Ultis.Settings.ListType == "Loading_List")
                {
                    JobItems model = (JobItems)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "PendingLoad");
                }
                if (Ultis.Settings.ListType == "refuel_List")
                {
                    RefuelHistoryData model = (RefuelHistoryData)this.BindingContext;

                    summaryRecord = App.Database.GetSummarysAsync(model.recordId,"Refuel");
                }       
                if(Ultis.Settings.ListType == "Main_Menu")
                {
                    AppMenu model = (AppMenu)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.menuId,"MainMenu");
                }
                if (Ultis.Settings.ListType == "Log_History")
                {
                    Log model = (Log)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.logId, "Log");
                }
                if (Ultis.Settings.ListType == "Run_Sheet")
                {
                    JobItems model = (JobItems)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "HaulageHistory");
                }
                if (Ultis.Settings.ListType == "Pending_Collection")
                {
                    ListObject model = (ListObject)this.BindingContext;
                    summaryRecord = App.Database.GetSummarysAsync(model.Id, "pendingCollection");
                }

                AbsoluteLayout absoluteLayout = new AbsoluteLayout();
                StackLayout cellWrapper = new StackLayout()
                {
                    //Padding = new Thickness(10, 10, 10, 10),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                };

                bool firstSummaryLine = true;

                foreach(SummaryItems items in summaryRecord)
                {
                    Label label = new Label();
                    
                    if (items.Caption == "" || items.Caption == "Job No." || items.Caption == "Consignee")
                    {
                        label.Text = items.Value;
                    }
                    else if(items.Caption == "Action" || items.Display == false)
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

                absoluteLayout.Children.Add(cellWrapper);

                

                View = new Frame
                 {
                     Content = absoluteLayout,
                     HasShadow =  true,
                     Margin = 5,                    
                 };

                if (!(String.IsNullOrEmpty(color)))
                {
                    View.BackgroundColor = Color.FromHex(color);
                }
                
            }

        }
    }
}
