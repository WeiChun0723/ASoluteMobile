using System;
using System.Collections.Generic;
using System.Text;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomRenderer;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using ASolute_Mobile.WoosimPrinterService;
using ASolute_Mobile.WoosimPrinterService.library.Cmds;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using XLabs.Forms.Controls;

namespace ASolute_Mobile.HaulageScreen
{
    public partial class NewJobDetails : ContentPage
    {
        ListItems jobItem;
        List<DetailItems> jobDetails;
        string jobCode, bookingCode;
        bool connectedPrinter = false;


        public NewJobDetails(ListItems job)
        {
            InitializeComponent();

            if (jobItem == null)
            {
                jobItem = App.Database.GetJobRecordAsync(Ultis.Settings.SessionCurrentJobId);
                jobDetails = App.Database.GetDetailsAsync(Ultis.Settings.SessionCurrentJobId);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (connectedPrinter == true)
            {
                DependencyService.Get<IBthService>().disconnBTDevice();
            }
        }

        void PageContent()
        {

        }


        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            var field = sender as CustomEntry;

            switch (field.StyleId)
            {
                case "contPrefix":
                    if (contPrefix.Text.Length == 4)
                    {
                        contNum.Focus();
                    }
                    break;

                case "contNum":
                    if (contNum.Text.Length == 7)
                    {
                        contNum.Unfocus();
                    }
                    break;

                case "mgwEntry":
                    if (mgwEntry.Text.Length == 5)
                    {
                        tareEntry.Focus();
                    }
                    break;
            }
        }

        void Handle_CheckedChanged(object sender, XLabs.EventArgs<bool> e)
        {
            var chkBox = sender as CheckBox;

            switch (chkBox.StyleId)
            {
                case "chkYes":
                    if (chkNo.Checked && chkYes.Checked)
                    {
                        chkNo.Checked = false;

                    }
                    if (TrailerDetailGrid.IsVisible)
                    {
                        mgwEntry.Focus();
                    }
                    break;

                case "chkNo":
                    if (chkYes.Checked && chkNo.Checked)
                    {
                        chkYes.Checked = false;

                    }
                    break;
            }

        }

        async void IconTapped(object sender, EventArgs e)
        {
            var image = sender as Image;

            switch(image.StyleId)
            {
                case "phone_icon":
                    Device.OpenUri(new Uri(String.Format("tel:{0}", jobItem.TelNo)));
                    break;

                case "map_icon":
                    Device.OpenUri(new Uri(String.Format("geo:{0}", jobItem.Latitude + "," + jobItem.Longitude)));
                    break;

                case "barCode_icon":
                    await PopupNavigation.Instance.PushAsync(new BarCodePopUp(jobCode, bookingCode));
                    break;

                case "print_icon":
                    PrintConsigmentNote();
                    break;

            }
        }

        async void PrintConsigmentNote()
        {
            print.IsVisible = true;

            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getConsigmentNoteURL(Ultis.Settings.SessionCurrentJobId), this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (response.IsGood)
            {
                if (response.Result.Count != 0)
                {
                    try
                    {
                        if (connectedPrinter == false)
                        {
                            bool x = await DependencyService.Get<IBthService>().connectBTDevice("00:15:0E:E6:25:23");

                            if (!x)
                            {
                                Device.BeginInvokeOnMainThread( () =>
                                {
                                    DisplayAlert("Error", "Unable connect", "OK");
                                });
                            }
                            else
                            {
                                connectedPrinter = true;
                            }
                        }

                        System.IO.MemoryStream buffer = new System.IO.MemoryStream(512);
                        string detail = "";
                        int detailCount = 0;
                        foreach (string details in response.Result)
                        {
                            detailCount++;

                            if (details.Contains("<H>"))
                            {
                                detail = details.Replace("<H>", "");
                                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT02, false));
                            }
                            else if (details.Contains("<B>"))
                            {
                                detail = details.Replace("<B>", "");
                                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                            }
                            else
                            {
                                detail = details;
                                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, false, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                            }


                            if (detailCount == response.Result.Count)
                            {
                                detail = detail + "\r\n\r\n";
                            }
                            else
                            {
                                detail = detail + "\r\n";
                            }

                            WriteMemoryStream(buffer, Encoding.ASCII.GetBytes(detail));
                        }

                        WriteMemoryStream(buffer, WoosimPageMode.print());

                        DependencyService.Get<IBthService>().WriteComm(buffer.ToArray());
                    }
                    catch (Exception error)
                    {
                        await DisplayAlert("Error", error.Message, "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert("Json Error", response.Message, "OK");
            }

            print.IsVisible = false;
        }

        async private void WriteMemoryStream(System.IO.MemoryStream stream, byte[] data)
        {
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}
