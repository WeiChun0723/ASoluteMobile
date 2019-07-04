using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
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
        public EquipmentInquiry ()
		{
			InitializeComponent ();
            Title = "Equipment Inquiry";
		}

        public void convertUpper(object sender, TextChangedEventArgs e)
        {
            string upperCase = equipmentID.Text.ToUpper();
            equipmentID.Text = upperCase;

        }

        public async void checkEquipment(object sender, EventArgs e)
        {
            loading.IsVisible = true;

            if (!(String.IsNullOrEmpty(equipmentID.Text)))
            {
                try
                {
                    var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getEquipmentURL(equipmentID.Text), this);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

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
                    
                    List <EqDetails> eqDetailsRow = new List<EqDetails>();
                    for(int j = 0; j < 1; j++)
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

                            Label label = new Label
                            {
                                Text = items.Caption + ": " + items.Value
                            };
                          
                            label.FontAttributes = FontAttributes.Bold;
                            

                            cellWrapper.Children.Add(label);

                        }

                        absoluteLayout.Children.Add(cellWrapper);


                        return new ViewCell { View = absoluteLayout };

                    });

                    euipmentList.ItemsSource = row;
                    euipmentList.HasUnevenRows = true;
                    euipmentList.ItemTemplate = Template;
                }
                catch (HttpRequestException)
                {
                    await DisplayAlert("Unable to connect", "Please try again later", "Ok");
                }
            }
            else
            {
                await DisplayAlert("Missing field", "Please key in all mandatory field", "OK");
            }

            loading.IsVisible = false;
        }

    }

    class EqDetails
    {
        public string details { get; set; }
    }
}