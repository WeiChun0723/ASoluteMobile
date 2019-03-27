using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.TransportScreen
{
    public partial class CargoItem : ContentPage
    {
        List<clsKeyValue> keyValue;
        List<clsCargoItem> cargoAdded = new List<clsCargoItem>();
        int cargoAmount;
        public CargoItem(string jobNo)
        {
            InitializeComponent();

            Title = "Add Cargo";

            subJobNo.Text = jobNo;
            UOMList();
        }


        public void desc_Completed(object sender, EventArgs e)
        {
            mark.Focus();

        }

        public void mark_Completed(object sender, EventArgs e)
        {
            quantity.Focus();

        }

        public void quantity_Completed(object sender, EventArgs e)
        {
            weight.Focus();

        }

        public void weight_Completed(object sender, EventArgs e)
        {
            lengthEntry.Focus();

        }

        public void length_Completed(object sender, EventArgs e)
        {
            widthEntry.Focus();

        }

        public void width_Completed(object sender, EventArgs e)
        {
            heightEntry.Focus();

        }


        public async void weightText(object sender, TextChangedEventArgs e)
        {
          
            try
            {
                if(!(String.IsNullOrEmpty(weight.Text)))
                {
                    if (Convert.ToDouble(weight.Text) <= 2000)
                    {
                        string inputValue = weight.Text;

                        if (weight.Text.Contains("."))
                        {
                            string[] value = weight.Text.Split('.');
                            string decimalValue = value[1];
                            if (decimalValue.Length > 2)
                            {
                                decimalValue = decimalValue.Remove(decimalValue.Length - 1);
                            }


                            weight.Text = value[0] + "." + decimalValue;
                        }
                        else
                        {
                            weight.Text = inputValue;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Value cant more than 2000.", "OK");
                        string inputWeight = weight.Text;
                        weight.Text = inputWeight.Remove(inputWeight.Length - 1);
                    }
                }
                 
            }
            catch
            {
                await DisplayAlert("Error", "Value in wrong format.", "OK");
               
            }
         

        }

        public  void quantityText(object sender, TextChangedEventArgs e)
        {

            try
            {
                int _text = Convert.ToInt16(quantity.Text);
                string value = quantity.Text;
                if (quantity.Text.Length > 3)
                {

                    value = value.Remove(value.Length - 1);

                }
                quantity.Text = value;
            }
            catch
            {
                string[] integer = quantity.Text.Split('.');
                quantity.Text = integer[0];
               
            }
           
        }

        public async void lengthText(object sender, TextChangedEventArgs e)
        {
            try
            {
                int _text = Convert.ToInt16(lengthEntry.Text);
                string value = lengthEntry.Text;
                if (lengthEntry.Text.Length > 3)
                {
                    value = value.Remove(value.Length - 1);

                }

                if (Convert.ToInt16(value) > 300)
                {
                    await DisplayAlert("Error", "Value cannot more than 300.", "OK");

                    lengthEntry.Text = value.Remove(value.Length - 1);
                }
                else
                {
                    lengthEntry.Text = value;
                }
               
            }
            catch
            {
                string[] integer = lengthEntry.Text.Split('.');
                lengthEntry.Text = integer[0];

            }
        }

        public async void widthText(object sender, TextChangedEventArgs e)
        {
            try
            {
                int _text = Convert.ToInt16(widthEntry.Text);
                string value = widthEntry.Text;
                if (widthEntry.Text.Length > 3)
                {
                    value = value.Remove(value.Length - 1);
                }

                if (Convert.ToInt16(value) > 300)
                {
                    await DisplayAlert("Error", "Value cannot more than 300.", "OK");

                    widthEntry.Text = value.Remove(value.Length - 1);
                }
                else
                {
                    widthEntry.Text = value;
                }
               
            }
            catch
            {
                string[] integer = widthEntry.Text.Split('.');
                widthEntry.Text = integer[0];

            }
        }

        public async void heightText(object sender, TextChangedEventArgs e)
        {
            try
            {
                int _text = Convert.ToInt16(heightEntry.Text);
                string value = heightEntry.Text;
                if (heightEntry.Text.Length > 3)
                {
                    value = value.Remove(value.Length - 1);


                }

                if(Convert.ToInt16(value) > 300)
                {
                    await DisplayAlert("Error", "Value cannot more than 300.", "OK");

                    heightEntry.Text = value.Remove(value.Length - 1);
                }
                else
                {
                    heightEntry.Text = value;
                }

            }
            catch
            {
                string[] integer = heightEntry.Text.Split('.');
                heightEntry.Text = integer[0];

            }
        }

        public async void addCargoItem(object sender, EventArgs e)
        {
            try
            {
                if (!(String.IsNullOrEmpty(cargoDesc.Text)) && !(String.IsNullOrEmpty(quantity.Text)) && !(String.IsNullOrEmpty(weight.Text)) && !(String.IsNullOrEmpty(lengthEntry.Text))
             && !(String.IsNullOrEmpty(widthEntry.Text)) && !(String.IsNullOrEmpty(heightEntry.Text)) && UOMPicker.SelectedIndex != -1)
                {
                       
                            if (Convert.ToDouble(weight.Text) <= 2000)
                            {
                                if (Convert.ToInt16(lengthEntry.Text) <= 300 && Convert.ToInt16(widthEntry.Text) <= 300 && Convert.ToInt16(heightEntry.Text) <= 300)
                                {
                                    clsCargoItem cargo = new clsCargoItem();

                                    cargo.OrderNo = subJobNo.Text;
                                    cargo.Description = cargoDesc.Text;
                                    cargo.Qty = Convert.ToInt32(quantity.Text);
                                    cargo.Uom = keyValue[UOMPicker.SelectedIndex].Key;
                                    cargo.Weight = Convert.ToDouble(weight.Text);
                                    cargo.Length = Convert.ToInt32(lengthEntry.Text);
                                    cargo.Width = Convert.ToInt32(widthEntry.Text);
                                    cargo.Height = Convert.ToInt32(heightEntry.Text);
                                    cargo.Marking = (!(String.IsNullOrEmpty(mark.Text))) ? mark.Text : "";

                                var content = await CommonFunction.PostRequestAsync(cargo, Ultis.Settings.SessionBaseURI, ControllerUtil.postCargoListURL());
                                clsResponse cargo_response = JsonConvert.DeserializeObject<clsResponse>(content);

                                    if (cargo_response.IsGood)
                                    {
                                        quantity.Text = "";
                                        weight.Text = "";
                                        lengthEntry.Text = "";
                                        widthEntry.Text = "";
                                        heightEntry.Text = "";
                                        cargoAdded.Clear();
                                        await DisplayAlert("Success", "Cargo Added", "OK");
                                        cargoAmount++;
                                        cargoCount.Text = "Item Added = " + cargoAmount;
                                    }
                                    else
                                    {
                                        await DisplayAlert("Error", cargo_response.Message, "OK");
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("Error", "Length, Width, Height cannot more than 300 each.", "OK");
                                }

                            }
                            else
                            {
                                await DisplayAlert("Error", "Weight can not more than 2000.", "OK");
                            }

                        }
 
                else
                {
                    await DisplayAlert("Error", "Please key in all the field", "OK");
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
          
        }


        public async void UOMList()
        {
            var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getUOMListURL());
            clsResponse uom_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(uom_response.IsGood)
            {
                keyValue = JObject.Parse(content)["Result"].ToObject<List<clsKeyValue>>();

                foreach (clsKeyValue reason in keyValue)
                {
                    UOMPicker.Items.Add(reason.Key);
                }
            }

        }
    }
}
