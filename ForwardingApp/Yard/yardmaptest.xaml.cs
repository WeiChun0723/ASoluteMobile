using System;
using System.Collections.Generic;
using Syncfusion.SfMaps.XForms;
using Xamarin.Forms;

namespace ASolute_Mobile.Yard
{
    public partial class yardmaptest : ContentPage
    {
        public yardmaptest()
        {
            InitializeComponent();

            List<TicketData> list = new List<TicketData>();
            for (int i = 1; i < 7; i++)
            {
                list.Add(new TicketData("" + i));
            }

            (this.Maps.Layers[0] as ShapeFileLayer).ItemsSource = list;
            (this.Maps.Layers[0] as ShapeFileLayer).ShapeSelectionChanged += MapsTicketBooking_ShapeSelectionChanged;
            (this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.CollectionChanged += (sender, e) =>
            {
                UpdateSelection();
            };

            this.ClearButton.Clicked += (sender, e) =>
            {
                if ((this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.Count != 0)
                {
                    (this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.Clear();
                    SelectedLabel.Text = "";
                    SelectedLabelCount.Text = "" + (this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.Count;
                    this.ClearButton.IsEnabled = false;
                    this.ClearButton.Opacity = 0.5;
                }
            };
        }

        private void MapsTicketBooking_ShapeSelectionChanged(object sender, ShapeSelectedEventArgs e)
        {
            TicketData data = e.Data as TicketData;
            if (data != null)
            {
                // if ((this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.Contains(e.Data))
                //  (this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.Remove(e.Data);
                this.ClearButton.Opacity = 1;
                this.ClearButton.IsEnabled = true;
                SelectedLabel.Text = data.SeatNumber;
            }
        }

        void UpdateSelection()
        {
            string selected = "";
            if ((Maps.Layers[0] as ShapeFileLayer).SelectedItems.Count == 0)
            {
                SelectedLabel.Text = selected;
                SelectedLabelCount.Text = " ";
                this.ClearButton.IsEnabled = false;
                this.ClearButton.Opacity = 0.5;
                
            }
            else
            {
                int count = 0;

                for (int i = 0; i < (this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.Count; i++)
                {
                    TicketData data = (this.Maps.Layers[0] as ShapeFileLayer).SelectedItems[i] as TicketData;

                        count++;
                        if ((this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.Count <= 1 && (this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.Count != 0)
                        {
                            selected += ("S" + data.SeatNumber);
                        }
                        else if (i == (this.Maps.Layers[0] as ShapeFileLayer).SelectedItems.Count - 1)
                        {
                            selected += ("S" + data.SeatNumber);
                        }
                        else
                        {
                            selected += ("S" + data.SeatNumber + ", ");
                        }

                        this.ClearButton.Opacity = 1;
                        this.ClearButton.IsEnabled = true;
                        SelectedLabel.Text = selected;
                }

                SelectedLabelCount.Text = "" + count;

            }
        }
    }

    public class TicketData
    {
        public TicketData(string Id)
        {
            SeatNumber = Id;
        }

        public string SeatNumber
        {
            get;
            set;
        }
    }
}
