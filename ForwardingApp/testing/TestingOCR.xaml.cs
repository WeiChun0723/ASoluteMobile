using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using ASolute.Mobile.Models;
using Rg.Plugins.Popup.Services;
using Newtonsoft.Json;
using Plugin.Media;
using Xamarin.Forms;
using Rg.Plugins.Popup;
using static ASolute_Mobile.TestingClass;

namespace ASolute_Mobile
{
    public partial class TestingOCR : ContentPage
    {
        const string subscriptionKey = "f41eb31aaf2e4fe98e0e382c22e6d0f7";
        const string uriBase =
            "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/ocr";
        List<string> ShippingCode = new List<string>
        {
            "KKFU",
            "HLXU",
            "EISU",
            "EMCU",
            "BMOU",
            "OOLU",
            "WHLU",
            "OEGU",
            "HDMU"
        };
        List<string> suggestions = new List<string>();
        List<People> ppl;

        public TestingOCR()
        {
            InitializeComponent();

            //CalculateCheckDigit("HDMU210851");
            MessagingCenter.Subscribe<App, string>((App)Application.Current, "ResultReturn",  (sender, arg) => {
                resultField.Text = arg ;
            });
            
            ppl = new List<People>
            {
                new People{name = "marry"},
                new People{name = "john"}
            };

            dataGrid.ItemsSource = ppl;

            comboBox.ComboBoxSource = ShippingCode;

        }

        private async void OnFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchKey = e.NewTextValue;

                if (string.IsNullOrEmpty(searchKey))
                {
                    dataGrid.ItemsSource = ppl;
                }

                else
                {
                    try
                    {
                        dataGrid.ItemsSource = ppl.Where(x => x.name.Contains(searchKey));
                    }
                    catch
                    {
                        await DisplayAlert("Error", "Please try again", "OK");
                    }
                }
            }
            catch
            {
                await DisplayAlert("Error", "suoer boring dude when is my package will arrive", "OK");
            }

        }

        async void Handle_GridTapped(object sender, Syncfusion.SfDataGrid.XForms.GridTappedEventArgs e)
        {
            People test = new People();
            test = ((People)e.RowData);

            await DisplayAlert("HI",test.name, "ok");
        }
       
        //ppl[dataGrid.SelectedIndex - 1].name

        async void google_api(object sender, System.EventArgs e)
        {

            try
            {
                suggestions.Clear();
                resultField.Text = string.Empty;
                prefix.Text = string.Empty;

                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    //content page refer to the page that call this function (this)
                    await DisplayAlert("No Camera", "No camera available", "OK");
                    return;
                }

                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions { Directory = "App_Image" });
                string filePath = file.AlbumPath;

                if (file == null)
                    return;

                byte[] imagesAsBytes;
                using (var memoryStream = new MemoryStream())
                {
                    file.GetStream().CopyTo(memoryStream);
                    file.Dispose();
                    imagesAsBytes = memoryStream.ToArray();
                }

                string image64 = Convert.ToBase64String(imagesAsBytes);

                TestingClass.Image image = new TestingClass.Image
                {
                    content = image64
                };

                TestingClass.Feature feature = new TestingClass.Feature
                {
                    type = "TEXT_DETECTION",
                    maxResults = 5
                };

                TestingClass.Request request = new TestingClass.Request
                {
                    image = image,

                };
                request.features = new List<TestingClass.Feature>();
                request.features.Add(feature);

                TestingClass.RootObject root = new TestingClass.RootObject();
                root.requests = new List<TestingClass.Request>();
                root.requests.Add(request);

                busyindicator.IsVisible = true;
                //loading.IsRunning = true;

                var client = new HttpClient();

                var uri = "https://vision.googleapis.com/v1/images:annotate?key=AIzaSyBKOwUpA5uP6PVDS5QXN_G1-PLSXiYiKVo";
                var content = JsonConvert.SerializeObject(root);
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, httpContent);
                var reply = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(reply);

                TestingClass.Google json_response = JsonConvert.DeserializeObject<TestingClass.Google>(reply);

                string google_result = json_response.responses[0].textAnnotations[0].description;

                StringBuilder result = new StringBuilder(google_result);

                string containerString1 = result
                    .Replace(" ", string.Empty)
                    .ToString();

                string containerString2 = containerString1.Replace("\n", string.Empty);

                string container = "";
                if (containerString2.Length >= 11)
                {
                    container = containerString2.Substring(0, 11);
                }
                else
                {
                    container = containerString2;
                }

                if (google_result != null)
                {
                    bool chkPre = container.Substring(0, 4).Any(char.IsDigit);
                    bool chkNum = container.Substring(4, 6).Any(char.IsLetter);
                    string contPrefix = container.Substring(0, 4), contNumber = container.Substring(4, 7);
                   
                    if (!chkPre && !chkNum)
                    {
                        clsCommonFunc checker = new clsCommonFunc();
                        bool checking = checker.CheckContainerDigit(container);

                        if(checking)
                        {
                            resultField.Text = container;
                        }
                        else
                        {
                            string possibleResult = "";

                                clsCommonFunc tryCheck = new clsCommonFunc();
                                bool tryCheckResult = tryCheck.CheckContainerDigit(contPrefix + contNumber);

                                if (tryCheckResult)
                                {
                                    possibleResult = contPrefix + contNumber;
                                    //resultField.Text = container;
                                }
                                else
                                {
                                    int checkDigit = CalculateCheckDigit(container);

                                    bool finalCheck = ContainerDigitCheck(contPrefix + contNumber.Remove(6) + checkDigit);

                                    if (finalCheck)
                                    {
                                    //resultField.Text = contPrefix + contNumber.Remove(6) + checkDigit;
                                        possibleResult = contPrefix + contNumber.Remove(6) + checkDigit;
                                    }
                                }

                            OCRPopUp oCRPop = new OCRPopUp(possibleResult);

                            await PopupNavigation.Instance.PushAsync(oCRPop);
                        }
                    }
                   else
                    {
                        resultField.Text = containerString2;
                    }

                }
                else
                {
                    resultField.Text = "undefined";
                }

                loading.IsRunning = false;
                busyindicator.IsVisible = false;
              

            }
            catch(Exception ex)
            {
                await DisplayAlert("OK", ex.Message, "OK");
                loading.IsRunning = false;
            }
        }

        bool ContainerDigitCheck(string containerNum) 
        {
            clsCommonFunc check = new clsCommonFunc();
            bool checkResult = check.CheckContainerDigit(containerNum);

            return checkResult;
        }

        int CalculateCheckDigit(string containerNum)
        {
            string first_Letter = containerNum.Substring(0, 1);
            string second_Letter = containerNum.Substring(1, 1);
            string third_Letter = containerNum.Substring(2, 1);
            string fourth_Letter = containerNum.Substring(3, 1);
            int first_Digit = Convert.ToInt16(containerNum.Substring(4, 1));
            int second_Digit = Convert.ToInt16(containerNum.Substring(5, 1));
            int third_Digit = Convert.ToInt16(containerNum.Substring(6, 1));
            int fourth_Digit = Convert.ToInt16(containerNum.Substring(7, 1));
            int fifth_Digit = Convert.ToInt16(containerNum.Substring(8, 1));
            int six_Digit = Convert.ToInt16(containerNum.Substring(9, 1));

            int totalSum = CharValue(first_Letter) + (CharValue(second_Letter) * 2) + (CharValue(third_Letter) * 4) + (CharValue(fourth_Letter) * 8) +
                           (first_Digit * 16) + (second_Digit * 32) + (third_Digit * 64) + (fourth_Digit * 128) + (fifth_Digit * 256) + (six_Digit * 512);

            int changeSum = Convert.ToInt32((totalSum / 11) * 11);

            int checkDigit = totalSum - changeSum;

            if(checkDigit == 10)
            {
               return checkDigit = 0;
            }

            return checkDigit;
        }

        int CharValue(string letter)
        {
            int value = 0;

            switch(letter)
            {
                case "A" :
                    value = 10;
                    break;

                case "B":
                    value = 12;
                    break;

                case "C":
                    value = 13;
                    break;

                case "D":
                    value = 14;
                    break;

                case "E":
                    value = 15;
                    break;

                case "F":
                    value = 16;
                    break;

                case "G":
                    value = 17;
                    break;

                case "H":
                    value = 18;
                    break;

                case "I":
                    value = 19;
                    break;

                case "J":
                    value = 20;
                    break;

                case "K":
                    value = 21;
                    break;

                case "L":
                    value = 23;
                    break;

                case "M":
                    value = 24;
                    break;

                case "N":
                    value = 25;
                    break;

                case "O":
                    value = 26;
                    break;

                case "P":
                    value = 27;
                    break;

                case "Q":
                    value = 28;
                    break;

                case "R":
                    value = 29;
                    break;

                case "S":
                    value = 30;
                    break;

                case "T":
                    value = 31;
                    break;

                case "U":
                    value = 32;
                    break;

                case "V":
                    value = 34;
                    break;

                case "W":
                    value = 35;
                    break;

                case "X":
                    value = 36;
                    break;

                case "Y":
                    value = 37;
                    break;

                case "Z":
                    value = 38;
                    break;

            }

            return value;
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            await DisplayAlert("test", qwer.Text + comboBox.Text, "OK");
            
           /* try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    //content page refer to the page that call this function (this)
                    await DisplayAlert("No Camera", "No camera available", "OK");
                    return;
                }

                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions { Directory = "App_Image" });
                string filePath = file.AlbumPath;

                if (file == null)
                    return;
                    

                byte[] imagesAsBytes;
                using (var memoryStream = new MemoryStream())
                {
                    file.GetStream().CopyTo(memoryStream);
                    file.Dispose();
                    imagesAsBytes = memoryStream.ToArray();
                }


               string requestParameters = "language=en&detectOrientation=true"; 

                HttpResponseMessage response;

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                client.BaseAddress = new Uri(uriBase);
                var uri = "?" + requestParameters;
                using (ByteArrayContent content = new ByteArrayContent(imagesAsBytes))
                {
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    response = await client.PostAsync(uri, content);
                }
               
                var reply = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(reply);

                TestingClass.testing json_response = JsonConvert.DeserializeObject<TestingClass.testing>(reply);

                string result = "";

                foreach(TestingClass.Region region in  json_response.regions)
                {
                    foreach(TestingClass.Line line in region.lines)
                    {
                        foreach (TestingClass.Word word in line.words)
                        {
                            result += word.text;
                        }
                    }
                }

                if(result != null || result != "")
                {
                    resultField.Text = result;
                }
                else
                {
                    resultField.Text = "undefined";
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Reminder", ex.Message, "OK");
            }*/
        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        async void Handle_Refreshing(object sender, System.EventArgs e)
        {
            await DisplayAlert("OK", "test success", "ok");
            pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {
           
            pullToRefresh.IsRefreshing = false;
        }
    }
}
