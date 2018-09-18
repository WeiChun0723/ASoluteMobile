using System;
using System.Collections.Generic;
using ASolute_Mobile.Models;
using Xamarin.Forms;

namespace ASolute_Mobile.HaulageScreen
{
    public partial class JobDetail : ContentPage
    {
        public JobList previousPage;
        double imageWidth;
        List<DetailItems> jobDetails;
        string currentJobId = Ultis.Settings.SessionCurrentJobId, jobNo, actionID, actionMessage;
        JobItems jobItem;

        public JobDetail(string ID, string Message)
        {
            InitializeComponent();
            actionID = ID;
            actionMessage = Message;
            Ultis.Settings.ActionID = ID;
            Ultis.Settings.App = "Haulage";

            imageWidth = App.DisplayScreenWidth / 3;
            imageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(imageWidth) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            PageContent();

            Ultis.Settings.MenuAction = "Job_List";
        }

        public void PageContent()
        {
            if (jobItem == null)
            {
                jobItem = App.Database.GetItemAsync(currentJobId);
                jobDetails = App.Database.GetDetailsAsync(currentJobId);
            }

            if (!(String.IsNullOrEmpty(jobItem.Title)))
            {
                Title = jobItem.Title;
            }

            foreach (DetailItems detailItem in jobDetails)
            {
                Label label = new Label();

                if (detailItem.Caption == "Pickup" || detailItem.Caption == "Drop-off")
                {
                    label.Text = detailItem.Caption + ":  " + detailItem.Value;
                    jobNo = detailItem.Caption + " :  " + detailItem.Value;
                    label.FontAttributes = FontAttributes.Bold;
                }
                else if (detailItem.Caption == "")
                {
                    label.Text = detailItem.Value;
                    label.FontAttributes = FontAttributes.Bold;
                }
                else
                {
                    label.Text = detailItem.Caption + ":  " + detailItem.Value;
                    label.FontAttributes = FontAttributes.Bold;
                }

                StackLayout stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start, Padding = new Thickness(0, 5, 0, 5) };
                stackLayout.Children.Add(label);
                jobDetailsStackLayout.Children.Add(stackLayout);
            }

        }
    }
}
