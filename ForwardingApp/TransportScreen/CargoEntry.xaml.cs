﻿using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CargoEntry : ContentPage
	{
        //public JobList previousPage;
        public static string jobRecordID = "";
        public string uri = "";
        List<JobNoList> jobValueList = new List<JobNoList>();

        public CargoEntry (string truckID, string recordID, string title)
		{
			InitializeComponent ();
            equipmentNo.Text = truckID;
            jobRecordID = recordID;
            App.Database.deleteJobNo();
          
            Title = (title == "CargoRec") ? "Cargo Receiving Entry" : "Cargo Loading Entry";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            jobValueList = App.Database.GetJobNo(jobRecordID,false);
            cargoList.ItemsSource = jobValueList;

        }

        public async void Cargo(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new TransportScreen.CargoItem(((JobNoList)e.Item).JobNoValue));
        }

        protected override bool OnBackButtonPressed()
        {
            if(jobValueList.Count != 0)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (await DisplayAlert("Reminder", "Are you sure you want to close this screen without submitting the data?", "Yes", "No"))
                    {
                        base.OnBackButtonPressed();

                        await Navigation.PopAsync();
                    }

                });

            }
            else
            {
                Navigation.PopAsync();
            }

            return true;
        } 

        public async void JobNoScan(object sender, EventArgs e)
        {
            var scanPage = new ZXingScannerPage();
            await Navigation.PushAsync(scanPage);

            scanPage.OnScanResult += (result) =>
            {
              
                Device.BeginInvokeOnMainThread( () =>
                {

                    JobNoList(result.Text);               

                });
             
            };

        }

        public async void JobNoAdd(object sender, EventArgs e)
        {

            if(jobNo.Text != null)
            {
                JobNoList(jobNo.Text);
                jobValueList = App.Database.GetJobNo(jobRecordID,false);
                cargoList.ItemsSource = jobValueList;

                jobNo.Text = string.Empty;
            }
            else
            {
                await DisplayAlert("Empty", "The job no field can't be empty", "OK");
            }
        }

        public async void JobNoList(string JobNo)
        {
            if(!(String.IsNullOrEmpty(JobNo)))
            {
                JobNoList existingJobNo = App.Database.GetJobNoAsync(JobNo);

                if (existingJobNo == null)
                {
                    existingJobNo = new JobNoList();
                    displayToast("New job added to list");
                }
                else
                {
                    displayToast("Job already exist in list");
                }

                existingJobNo.JobNoValue = JobNo;
                existingJobNo.JobId = jobRecordID;
                existingJobNo.Uploaded = false;
                App.Database.SaveJobNoAsync(existingJobNo);
            }
            else
            {
                await DisplayAlert("Error", "Value cannot be empty", "OK");
            }

        }

       
        public async void confirmJobNo(object sender, EventArgs e)
        {
        
             try
              {
                  List<JobNoList> jobValue = new List<JobNoList>(App.Database.GetJobNo());
                  List<string> jobsData = new List<string>();
                  for(int i = 0; i < jobValue.Count; i++)
                  {
                      jobsData.Add(jobValue[i].JobNoValue);
                  }

                  var client = new HttpClient();
                  string baseAddress = Ultis.Settings.SessionBaseURI;
                  var url = baseAddress + ControllerUtil.postNewCargoRecordURL(jobRecordID);
                  Uri uri = new Uri(url);              
                  var content = JsonConvert.SerializeObject(jobsData);
                  var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                  var response = await client.PostAsync(uri, httpContent);
                  var reply = await response.Content.ReadAsStringAsync();
                  Debug.WriteLine(reply);

                  clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(reply);

                  if(json_response.IsGood == true)
                  {
                      
                      await DisplayAlert("Success", "Data uploaded", "OK");
                      await Navigation.PopAsync();
                  }
                  else
                  {
                      await DisplayAlert("Error", json_response.Message, "OK");
                  }
              }
              catch(HttpRequestException)
              {
                  await DisplayAlert("Unable to connect", "Please try again later", "Ok");
              }
              catch(Exception exception)
              {
                  await DisplayAlert("Error", exception.Message, "Ok");
              }
        }

       public void displayToast(string message)
        {
            var toastConfig = new ToastConfig(message);
            toastConfig.SetDuration(2000);
            toastConfig.Position = 0;
            toastConfig.SetMessageTextColor(System.Drawing.Color.FromArgb(0, 0, 0));
            if(message == "New job added to list")
            {
                toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(0, 128, 0));
            }
            else if(message == "Job already exist in list")
            {
                toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(255, 0, 0));
            }
            UserDialogs.Instance.Toast(toastConfig);
        }
    }
}