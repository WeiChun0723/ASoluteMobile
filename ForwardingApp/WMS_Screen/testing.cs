using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.WMS_Screen
{


    public class testing : ListViewCommonScreen
    {

        ObservableCollection<AppMenu> records = new ObservableCollection<AppMenu>();
        StackLayout frameLayout = new StackLayout();
        public testing(string screen_Title)
        {
            Title = screen_Title;

            GetPickingList();


            listView.ItemTapped += async (sender, e) =>
            {
                //await Navigation.PushAsync(new WMS_Screen.TallyInList(((AppMenu)e.Item).name));
                await DisplayAlert("test", ((AppMenu)e.Item).menuId, "K");
            };

            listView.Refreshing += (sender, e) =>
            {
                GetPickingList();
                listView.IsRefreshing = false;
            };
        }

        async void GetPickingList()
        {

            loading.IsVisible = true;
            records.Clear();
            
            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getPickingList());
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(response.IsGood)
            {
                List<clsDataRow> pickingList = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

                foreach (clsDataRow data in pickingList)
                {
                    string summary = "";
                    AppMenu record = new AppMenu
                    {
                        menuId = data.Id

                    };

                    if (!(String.IsNullOrEmpty(data.BackColor)))
                    {
                        record.background = data.BackColor;
                    }
                    else
                    {
                        record.background = "#ffffff";
                    }

                    StackLayout cellWrapper = new StackLayout()
                    {
                        //Padding = new Thickness(10, 10, 10, 10),
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand
                    };

                    int count = 0;
                    foreach (clsCaptionValue summaryItem in data.Summary)
                    {
                        Label label = new Label();

                        if (!(String.IsNullOrEmpty(summaryItem.Caption)))
                        {
                           
                           label.Text = summaryItem.Caption + " :  " + summaryItem.Value;

                        }

                        cellWrapper.Children.Add(label);

                        if (!(String.IsNullOrEmpty(data.BackColor)))
                        {
                            cellWrapper.BackgroundColor = Color.FromHex(data.BackColor);

                        }

                       

                        /*

                        if (!(String.IsNullOrEmpty(summaryItem.Caption)))
                        {
                            if (count == data.Summary.Count)
                            {
                                summary += summaryItem.Caption + " :  " + summaryItem.Value + System.Environment.NewLine;
                            }
                            else
                            {
                                summary += summaryItem.Caption + " :  " + summaryItem.Value + System.Environment.NewLine + System.Environment.NewLine;
                            }

                        }*/

                    }

                    listView.ItemTemplate = new DataTemplate(() =>
                    {
                        ViewCell viewCell = new ViewCell();

                        Frame frame = new Frame
                        {
                            Content = frameLayout,
                            HasShadow = true,
                            Margin = 5,
                        };

                        frame.SetBinding(BackgroundColorProperty, "background");
                        viewCell.View = frame;

                        return viewCell;
                    });
                    record.summary = summary;
                    records.Add(record);
                }
                loading.IsVisible = false;
                loadPickingList();
            }
            else
            {
                await DisplayAlert("Error", response.Message, "OK");
            }

        }


        public void loadPickingList()
        {
            listView.HasUnevenRows = true;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                ViewCell viewCell = new ViewCell();

                Frame frame = new Frame
                {
                    Content = frameLayout,
                    HasShadow = true,
                    Margin = 5,
                };

                frame.SetBinding(BackgroundColorProperty, "background");
                viewCell.View = frame;

                return viewCell;
            });
            frameLayout.SetBinding(BackgroundColorProperty, "background");
            Label label = new Label();
            label.FontAttributes = FontAttributes.Bold;
            label.SetBinding(Label.TextProperty, "summary");
            frameLayout.Children.Add(label);

            listView.ItemsSource = records;

            if (records.Count == 0)
            {
                listView.IsVisible = true;
                image.IsVisible = true;
            }
            else
            {
                listView.IsVisible = true;
                image.IsVisible = false;
            }
        }

    }
}

