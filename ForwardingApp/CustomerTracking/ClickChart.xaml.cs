using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Com.Syncfusion.Charts;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class ClickChart : ContentPage
    {
        int test;
        ObservableCollection<Persons> Data;

        public ClickChart()
        {
           
            InitializeComponent();

            Data = new ObservableCollection<Persons>()
            {
               // new Persons { Name = "David", Height = 55 },
              //  new Persons { Name = "Michael", Height = 15 },
              //  new Persons { Name = "Steve", Height = 20 },
              //  new Persons { Name = "Joel", Height = 10 }
            };
             
            bar.ItemsSource = Data;
            bar.XBindingPath = "Name";
            bar.YBindingPath = "Height";

        }

        public async void testing(object sender, ChartSelectionChangingEventArgs e)
        {
            e.Cancel = true;

            if (e.SelectedDataPointIndex > -1)
            {
                test = e.SelectedDataPointIndex;
                await DisplayAlert("Success", Data[test].Name, "OK");
            }
        }

    }
}
