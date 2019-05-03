using System;
using System.Collections.Generic;
using System.IO;
using ASolute_Mobile.Models;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using PCLStorage;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace ASolute_Mobile.CommonScreen
{
    public partial class UserInfo : ContentPage
    {
        public UserInfo()
        {
            InitializeComponent();

            Title = "User Info";

            userID.Text = "User ID: " + Ultis.Settings.SessionUserItem.DriverId;
            userName.Text = "User Name: " + Ultis.Settings.SessionUserItem.UserName;
            userVehicle.Text = "User Vehicle: " + Ultis.Settings.SessionUserItem.TruckId;


            ShowProfilePicture();
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            loading.IsVisible = true;

            var button = sender as SfButton;

            switch (button.StyleId)
            {
                case "takeImageButton":
                    await CommonFunction.StoreImages(Ultis.Settings.SessionUserItem.DriverId, this, "ProfilePic");
                    ShowProfilePicture();
                    break;

                case "chooseImageButton":
                    try
                    {
                        await CrossMedia.Current.Initialize();

                        if (!CrossMedia.Current.IsPickPhotoSupported)
                        {
                            //content page refer to the page that call this function (this)
                            await DisplayAlert("No Camera", "No camera available", "OK");
                            return;
                        }

                        var mediaOption = new PickMediaOptions()
                        {
                            PhotoSize = PhotoSize.Medium

                        };

                        var selectedImageFile = await CrossMedia.Current.PickPhotoAsync(mediaOption);

                        if (selectedImageFile == null)
                        {
                            loading.IsVisible = false;
                            return;
                        }
                        else
                        {

                            App.Database.DeleteUserImage(Ultis.Settings.SessionUserItem.DriverId);
                            AppImage image = new AppImage();
                            image.id = Ultis.Settings.SessionUserItem.DriverId;
                            image.photoFileLocation = "";
                            image.Uploaded = false;

                            string photoFileName = HelperUtil.GetPhotoFileName(image.photoFileLocation);

                            image.photoFileName = photoFileName;
                            image.type = "ProfilePic";
                            byte[] imagesAsBytes;
                            using (var memoryStream = new MemoryStream())
                            {
                                selectedImageFile.GetStream().CopyTo(memoryStream);
                                selectedImageFile.Dispose();
                                imagesAsBytes = memoryStream.ToArray();
                            }

                            //resize the photo and store in different directory 
                            byte[] thumbnailByte;

                            thumbnailByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(imagesAsBytes, 720, 1080, 100);

                            image.imageData = thumbnailByte;
                            App.Database.SaveRecordImageAsync(image);
                        }

                        ShowProfilePicture();
                    }
                    catch (Exception ex)
                    {
                        // await DisplayAlert("Error", ex.Message, "OK");
                    }
                    break;
            }
        }

        void ShowProfilePicture()
        {
            Device.BeginInvokeOnMainThread(() =>
            {

                try
                {
                    AppImage image = App.Database.GetUserProfilePicture(Ultis.Settings.SessionUserItem.DriverId);
                    profilePic.Source =(image != null ) ? ImageSource.FromStream(() => new MemoryStream(image.imageData)) : "user_icon.png";
                }
                catch
                {

                }
            });

            loading.IsVisible = false;
        }

        async void ChooseProfilePicture()
        {
            try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    //content page refer to the page that call this function (this)
                    await DisplayAlert("No Camera", "No camera available", "OK");
                    return;
                }

                var mediaOption = new PickMediaOptions()
                {
                    PhotoSize = PhotoSize.Medium
           
                };

                var selectedImageFile = await CrossMedia.Current.PickPhotoAsync(mediaOption);

                if (selectedImageFile == null)
                {
                    loading.IsVisible = false;
                    return;
                }
                else
                {

                    App.Database.DeleteUserImage(Ultis.Settings.SessionUserItem.DriverId);
                    AppImage image = new AppImage();
                    image.id = Ultis.Settings.SessionUserItem.DriverId;
                    image.photoFileLocation = "";
                    image.Uploaded = false;

                    string photoFileName = HelperUtil.GetPhotoFileName(image.photoFileLocation);

                    image.photoFileName = photoFileName;
                    image.type = "ProfilePic";
                    byte[] imagesAsBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        selectedImageFile.GetStream().CopyTo(memoryStream);
                        selectedImageFile.Dispose();
                        imagesAsBytes = memoryStream.ToArray();
                    }

                    //resize the photo and store in different directory 
                    byte[] thumbnailByte;

                    thumbnailByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(imagesAsBytes, 720, 1080, 100);

                    image.imageData = thumbnailByte;
                    App.Database.SaveRecordImageAsync(image);
                }

                ShowProfilePicture();
            }
            catch (Exception ex)
            {
                // await DisplayAlert("Error", ex.Message, "OK");
            }

        }
    }
}
