using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Syncfusion.XForms.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class EquipmentInquiry : ContentPage
    {
        string terminateDriverId = "";

        public EquipmentInquiry()
        {
            InitializeComponent();
            Title = "Equipment Inquiry";
        }

        public void convertUpper(object sender, TextChangedEventArgs e)
        {
            string upperCase = equipmentID.Text.ToUpper();
            equipmentID.Text = upperCase;
        }

        public async void Handle_Clicked(object sender, EventArgs e)
        {
            loading.IsVisible = true;
            

            var button = sender as SfButton;

            switch (button.StyleId)
            {
                case "btnConfirm":
                    btnTerminate.IsVisible = false;
                    equipmentDetails.IsVisible = false;
                    if (!(String.IsNullOrEmpty(equipmentID.Text)))
                    {
                        GetEquipmentDetail();
                    }
                    else
                    {
                        await DisplayAlert("Missing field", "Please key in all mandatory field", "OK");
                    }
                    break;

                case "btnTerminate":
                    var answer = await DisplayAlert("", "Terminate session?", "Yes", "No");

                    if (answer.Equals(true))
                    {
                        var terminate_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.terminateSessionURL(terminateDriverId), this);
                        clsResponse terminate_response = JsonConvert.DeserializeObject<clsResponse>(terminate_content);

                        if (terminate_response.IsGood)
                        {
                            await DisplayAlert("Success", "Session terminated", "OK");
                            btnTerminate.IsVisible = false;

                            GetEquipmentDetail();
                        }
                    }
                    break;
            }

            loading.IsVisible = false;
        }

        async void GetEquipmentDetail()
        {
            try
            {
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getEquipmentURL(equipmentID.Text), this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    List<clsCaptionValue> eqInfo = new List<clsCaptionValue>();

                    int count = 0;
                    for (int i = 0; i < response.Result.Count; i++)
                    {
                        string caption = response.Result[i]["Caption"];
                        string value = response.Result[i]["Value"];
                        bool display = response.Result[i]["Display"];
                        eqInfo.Add(new clsCaptionValue(caption, value, display));

                    }
                    count++;

                    List<EqDetails> eqDetailsRow = new List<EqDetails>();
                    for (int j = 0; j < 1; j++)
                    {
                        EqDetails listRow = new EqDetails();
                        listRow.details = "true";

                        eqDetailsRow.Add(listRow);
                    }

                    ObservableCollection<EqDetails> row = new ObservableCollection<EqDetails>(eqDetailsRow);

                    var Template = new DataTemplate(() =>
                    {

                        AbsoluteLayout absoluteLayout = new AbsoluteLayout();
                        StackLayout cellWrapper = new StackLayout()
                        {
                            Padding = new Thickness(10, 20, 20, 20),
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.FillAndExpand
                        };

                        foreach (clsCaptionValue items in eqInfo)
                        {
                            if (items.Caption == "DriverId" && !(String.IsNullOrEmpty(items.Caption)))
                            {
                                terminateDriverId = items.Value;
                                btnTerminate.IsVisible = true;
                            }

                            if (items.Display != false)
                            {
                                Label label = new Label
                                {
                                    Text = items.Caption + ": " + items.Value
                                };

                                label.FontAttributes = FontAttributes.Bold;


                                cellWrapper.Children.Add(label);
                            }
                        }


                        if (cellWrapper.Children.Count != 0)
                        {
                            absoluteLayout.Children.Add(cellWrapper);

                        }

                        return new ViewCell { View = absoluteLayout };
                    });

                    equipmentDetails.IsVisible = true;
                    equipmentDetails.ItemsSource = row;
                    equipmentDetails.HasUnevenRows = true;
                    equipmentDetails.ItemTemplate = Template;
                }
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Unable to connect", "Please try again later", "Ok");
            }
        }
    }

   

    class EqDetails
    {
        public string details { get; set; }
    }
}