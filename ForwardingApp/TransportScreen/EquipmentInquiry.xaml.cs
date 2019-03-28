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
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EquipmentInquiry : ContentPage
	{
        clsResponse newEqResponse = new clsResponse();

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
           /* string equipmentId = equipmentID.Text;

            if (NetworkCheck.IsInternet())
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);

                    var eqUri = ControllerUtil.getEquipmentURL(equipmentId);

                    var eqResponse = await client.GetAsync(eqUri);
                    var eqContent = await eqResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine(eqContent);

                    newEqResponse = JsonConvert.DeserializeObject<clsResponse>(eqContent);

                    List<clsCaptionValue> eqInfo = new List<clsCaptionValue>();

                    int count = 0;
                    for (int i = 0; i < newEqResponse.Result.Count; i++)
                    {
                        string caption = newEqResponse.Result[i]["Caption"]; 
                        string value = newEqResponse.Result[i]["Value"];
                        bool display = newEqResponse.Result[i]["Display"];
                        eqInfo.Add(new clsCaptionValue(caption, value, display));
                        
                    }
                    count++;
                    
                    List <ListObject> numberRow = new List<ListObject>();
                    for(int j = 0; j < count; j++)
                    {
                        ListObject listRow = new ListObject();
                        listRow.type = "true";

                        numberRow.Add(listRow);
                    }

                    ObservableCollection<ListObject> row = new ObservableCollection<ListObject>(numberRow);
                   
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
                await DisplayAlert("Reminder", "Currently offline cant search", "OK");
            }*/
        }

    }
}