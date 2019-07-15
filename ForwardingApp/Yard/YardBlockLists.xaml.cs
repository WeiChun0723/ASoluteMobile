using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Syncfusion.XForms.ProgressBar;

namespace ASolute_Mobile.Yard
{
    public partial class YardBlockLists : ContentPage
    {
        string blockLabel = "";

        public YardBlockLists(List<clsYardMapBlock> blocks)
        {
            InitializeComponent();

            Title = "Blocks";

            int column = 0, row = 0;

            foreach (clsYardMapBlock block in blocks)
            {
                if(!(block.Id.Contains("(")))
                {
                    blockLabel = block.Id + " " + block.Utilization + "%" + " ";
                }
               
                grid.RowDefinitions.Add(new RowDefinition { Height = 100 });

                Label label = new Label
                {
                    Text = blockLabel,
                    FontSize = 20,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(10,10,0,0)
                };

                SfLinearProgressBar linearProgressBar = new SfLinearProgressBar
                {
                    Progress = Convert.ToInt16(block.Utilization),
                    ProgressColor = Color.CornflowerBlue,
                    TrackColor = Color.LightBlue,
                    TrackHeight = 40,
                    Margin = new Thickness(0, 10, 10, 0),
                    CornerRadius = 10
                };

                var recognizer = new TapGestureRecognizer
                {
                    NumberOfTapsRequired = 1
                };      

                recognizer.Tapped += async (sender, e) =>
                {
					await Navigation.PushAsync(new YardBlockDetails(block.Id));	
				};
                linearProgressBar.GestureRecognizers.Add(recognizer);

                grid.Children.Add(label, column, row);
                column++;
                grid.Children.Add(linearProgressBar, column, row);
                row++;
                column = 0;
            }
        }

        void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {

        }

    }

}
