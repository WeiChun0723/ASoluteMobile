using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace ASolute_Mobile.HaulageScreen
{
    public partial class BarCodePopUp : PopupPage
    {
        public BarCodePopUp(string actionCode, string bookingCode)
        {
            InitializeComponent();

            export.Text = actionCode;
            booking.Text = bookingCode;
        }
    }
}
