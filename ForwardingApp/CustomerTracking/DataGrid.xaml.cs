using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using System.Linq;
using Syncfusion.SfDataGrid.XForms;
using Syncfusion.SfChart.XForms;

namespace ASolute_Mobile.CustomerTracking
{

    public partial class DataGrid : CarouselPage
    {
        bool firstLoad = true;
        int gridHeight;
        DateTime dataTime, previousTime;
        List<clsVolume> volumes;
        List<HaulageVolume> data = new List<HaulageVolume>();

        public DataGrid()
        {
            InitializeComponent();

            SelectedItem = Children[1];

            Title = DateTime.Now.ToString("MMMM yyyy");
            dataTime = DateTime.Now;
            GetHaulageVolume(DateTime.Now.ToString("yyyy-MM-dd"));
            previousTime = DateTime.Now;
        }

        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

          

            int index = Children.IndexOf(CurrentPage);
            if(!firstLoad)
            {
                if (index == 0)
                {
                    previousTime = dataTime;
                    dataTime = previousTime.AddMonths(-1);
                    GetHaulageVolume(dataTime.ToString("yyyy-MM-dd"));
                }
               
                else if (index == 2)
                {
                    previousTime = dataTime;
                    dataTime = previousTime.AddMonths(1);
                    GetHaulageVolume(dataTime.ToString("yyyy-MM-dd"));
                }

            }
        }

        public async void GetHaulageVolume(string date)
        {
            data.Clear();

            var volume_content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getHaulageVolumeURL(date));
            clsResponse volume_response = JsonConvert.DeserializeObject<clsResponse>(volume_content);

            if (volume_response.IsGood)
            {
                firstLoad = false;

                CurrentPage = Children[1];

                Title = dataTime.ToString("MMMM yyyy");

                volumes = JObject.Parse(volume_content)["Result"].ToObject<List<clsVolume>>();

                foreach(clsVolume volume in volumes)
                {
                    double revenue = Convert.ToDouble(volume.Revenue) / 1000;

                    HaulageVolume dataValue = new HaulageVolume
                    {
                        Entity = volume.Entity,
                        Job = volume.Job.ToString("N0"),
                        Revenue = revenue.ToString("N0") + " K"
                    };

                    data.Add(dataValue);
                }

                gridHeight = 0;

                if(volumes.Count == 1)
                {
                    gridHeight = 100;
                }
                else
                {
                    gridHeight = data.Count * 60;
                }

                PageContent();
            }
            else
            {
                await DisplayAlert("JsonError", volume_response.Message, "OK");
            }

        }

        public void PageContent()
        {
            StackLayout mainLayout = new StackLayout
            {
                Padding = new Thickness(10, 10, 10, 10)
            };

            mainLayout.Children.Clear();

            Image noData = new Image
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Source = "nodatafound.png",
                IsVisible = false
            };

          
            ActivityIndicator activityIndicator = new ActivityIndicator
            {
                IsRunning = true,
                IsVisible = true
            };

            SfDataGrid dataGrid = new SfDataGrid
            {
                HorizontalOptions = LayoutOptions.Center,
                HeightRequest = gridHeight,
                MinimumHeightRequest = gridHeight + 30,
                BackgroundColor = Color.Transparent
            };

            GridTextColumn entityColumn = new GridTextColumn
            {
                MappingName = "Entity",
                Width = 100
            };

            entityColumn.HeaderTemplate = new DataTemplate(() =>
            {
                var entityLabel = CreateLabel("Entity", true, "");

                return entityLabel;
            });

            GridTextColumn jobColumn = new GridTextColumn
            {
                MappingName = "Job",
                Width = 100
            };

            jobColumn.HeaderTemplate = new DataTemplate(() =>
            {
                var jobLabel = CreateLabel("Box", true, "");

                return jobLabel;
            });


            GridTextColumn revenueColumn = new GridTextColumn
            {
                MappingName = "Revenue",
                Width = 100,
            
            };

            revenueColumn.HeaderTemplate = new DataTemplate(() =>
            {
                var revenueLabel = CreateLabel("Revenue", true, "");

                return revenueLabel;
            });

            dataGrid.Columns.Add(entityColumn);
            dataGrid.Columns.Add(jobColumn);
            dataGrid.Columns.Add(revenueColumn);


            StackLayout category = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 20,
                HorizontalOptions = LayoutOptions.Center
            };

            category.Children.Add(CreateLabel("Box", false, "#FFCC00"));
            category.Children.Add(CreateLabel("Revenue", false, ""));

            SfChart chart = new SfChart
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = data.Count * 85,
                IsEnabled = false,

            };

            //Initializing Primary Axis
            CategoryAxis primaryAxis = new CategoryAxis();

            chart.PrimaryAxis = primaryAxis;

            //Initializing Secondary Axis
            NumericalAxis secondaryAxis = new NumericalAxis();

            chart.SecondaryAxis = secondaryAxis;

            BarSeries job = new BarSeries()
            {
                EnableAnimation = true,
                Color = Color.FromHex("#FFCC00")
            };

            BarSeries revenue = new BarSeries()
            {
               
                EnableAnimation = true,
                Color = Color.Green
            };

            if (data.Count == 0)
            {
                noData.IsVisible = true;
                dataGrid.IsVisible = false;
                chart.IsVisible = false;
                category.IsVisible = false;
            }
            else if(data.Count == 1)
            {
                job.Width = 0.3;
                revenue.Width = 0.3;
            }
            else if(data.Count == 2 )
            {
                job.Width = 0.35;
                revenue.Width = 0.35;
            }
            else if(data.Count == 3)
            {
                job.Width = 0.45;
                revenue.Width = 0.45;
            }
            else
            {
                job.Width = 0.7;
                revenue.Width = 0.7;
            }


            chart.Series.Add(job);
            chart.Series.Add(revenue);

            mainLayout.Children.Add(noData);
            mainLayout.Children.Add(activityIndicator);
            mainLayout.Children.Add(dataGrid);
            mainLayout.Children.Add(category);
            mainLayout.Children.Add(chart);

            dataGrid.ItemsSource = data;

            List<HaulageVolume> chartData = new List<HaulageVolume>();

            foreach(HaulageVolume haulageVolume in data)
            {
                string[] number = haulageVolume.Revenue.Split(' ');

                HaulageVolume volume = new HaulageVolume
                {
                    Entity = haulageVolume.Entity,
                    Job = haulageVolume.Job,
                    Revenue = number[0]
                };

                chartData.Add(volume);
            }

            chartData.Reverse();

            job.ItemsSource = chartData;
            job.XBindingPath = "Entity";
            job.YBindingPath = "Job";

            revenue.ItemsSource = chartData;
            revenue.XBindingPath = "Entity";
            revenue.YBindingPath = "Revenue";

            activityIndicator.IsRunning = false;
            activityIndicator.IsVisible = false;
            activityIndicator.IsEnabled = false;

            CurrentPage.Content = new ScrollView
            {
                Content = mainLayout
            };
        }

        public Label CreateLabel(string text, bool gridLabel, string textColor)
        {
            Label label = new Label();

            if (gridLabel)
            {

                label.Text = text;
                label.FontAttributes = FontAttributes.Bold;
                label.HorizontalTextAlignment = TextAlignment.Center;
                label.VerticalTextAlignment = TextAlignment.Center;
            }
            else
            {
                label.Text = text;
                label.TextColor = (text.Equals("Revenue")) ? Color.Green : Color.FromHex(textColor);
                label.FontAttributes = FontAttributes.Bold;
                label.FontSize = 20;
            }

            return label;
        }
    }
}
