using System;
using System.Collections.Generic;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class PieChart : ContentPage
    {
        string providerCode;
        List<string> pieData = new List<string>();
        List<clsCaptionValue> categories;
        int test = 0;

        public PieChart(string code)
        {
            InitializeComponent();
            Title = "Pie Chart";

            providerCode = code;

           
            GetData();
        }


        public async void GetData()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getCategoryList(providerCode));
            clsResponse provider_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(provider_response.IsGood)
            {
                categories = JObject.Parse(content)["Result"].ToObject<List<clsCaptionValue>>();

                foreach (clsCaptionValue category in categories)
                {
                    string quantity = new String(category.Value.Where(Char.IsDigit).ToArray());

                    if (!(category.Caption.Equals("AC")) && !(category.Caption.Equals("AD")))
                    {
                        pieData.Add(category.Value);
                    }

                    test += Convert.ToInt32(quantity);
                }
            }

            if(providerCode.Equals("PCLTEST"))
            {
                DrawPieChart();
            }
            else
            {
                DrawBarChart();
            }


        }

        public void DrawPieChart()
        {
            PlotModel Models = new PlotModel { Title = "Pie Chart" };

            var ps = new PieSeries
            {
                StrokeThickness = 2.0,
                InsideLabelPosition = 0.5,
                AngleSpan = 360,
                StartAngle = 0,
                TextColor = OxyColor.FromRgb(100, 100, 100),
                OutsideLabelFormat = "{2:0.0}%",
                TickLabelDistance = .25
            };

            foreach(string data in pieData)
            {
                string quantity = new String(data.Where(Char.IsDigit).ToArray());
                string[] cat = data.Split('(');

                ps.Slices.Add(new PieSlice(cat[0], Convert.ToInt32(quantity)) { IsExploded = false, Fill = OxyColor.FromRgb(255, 153, 153) });
            }
          

            Models.Series.Add(ps);

            Pie.Model = Models;

        }

        public void DrawBarChart()
        {
            /*  var model = new PlotModel { Title = "Bar Chart",  LegendPlacement = LegendPlacement.Outside,
                  LegendPosition = LegendPosition.BottomCenter,
                  LegendOrientation = LegendOrientation.Horizontal,
                  LegendBorderThickness = 0
              };

              List<BarItem> bars = new List<BarItem>();
              List<string> name = new List<string>();
              foreach(string data in pieData)
              {
                  string quantity = new String(data.Where(Char.IsDigit).ToArray());
                  string[] cat = data.Split('(');

                  bars.Add(new BarItem { Value = (Convert.ToInt32(quantity) / test ) });
                  name.Add(cat[0]);
              }

              var br = new BarSeries
              {
                  ItemsSource = bars,
                  LabelPlacement = LabelPlacement.Inside,
                  LabelFormatString = "{0:.00}%"
              };

              model.Series.Add(br);

              model.Axes.Add(new CategoryAxis
              {
                  Position = AxisPosition.Left,
                  Key = "NameAxis",
                  ItemsSource = name

              });*/

            var model = new PlotModel { Title = "Cake Type Popularity" };
            //generate a random percentage distribution between the 5
            //cake-types (see axis below)
            var rand = new Random();
            double[] cakePopularity = new double[5];
            for (int i = 0; i < 5; ++i)
            {
                cakePopularity[i] = rand.NextDouble();
            }
            var sum = cakePopularity.Sum();

            var barSeries = new BarSeries
            {
                ItemsSource = new List<BarItem>(new[]
{

new BarItem{ Value = (cakePopularity[0] / sum * 100) },
new BarItem{ Value = (cakePopularity[1] / sum * 100) },
new BarItem{ Value = (cakePopularity[2] / sum * 100) },
new BarItem{ Value = (cakePopularity[3] / sum * 100) },
new BarItem{ Value = (cakePopularity[4] / sum * 100) }
}),
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}%"
            };
            model.Series.Add(barSeries);
            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "CakeAxis",
                ItemsSource = new[]
            {
"Apple cake",
"Baumkuchen",
"Bundt Cake",
"Chocolate cake",
"Carrot cake"
}
            });

            Pie.Model = model;
        }
    }
}
