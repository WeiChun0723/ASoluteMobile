using System;
using System.Collections.Generic;
using System.IO;
using ASolute_Mobile.Models;
using ASolute_Mobile.Ultis;
using PCLStorage;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class ImageViewer : ContentPage
    {
        private AppImage appImage { get; set; }
        private PCLStorage.IFile actualImageFile;
        
        public ImageViewer(AppImage appImage)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            this.appImage = appImage;
        }

        protected override async void OnAppearing(){

			Grid grid = new Grid();
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            actualImageFile = await FileSystem.Current.GetFileFromPathAsync(appImage.photoFileLocation);
			Stream stream = await actualImageFile.OpenAsync(PCLStorage.FileAccess.Read);
			var image = new Image();
			byte[] imageByte;
			using (MemoryStream ms = new MemoryStream())
			{
				stream.Position = 0; // needed for WP (in iOS and Android it also works without it)!!
				stream.CopyTo(ms);  // was empty without stream.Position = 0;
				imageByte = ms.ToArray();
			}
			image.Source = ImageSource.FromStream(() => new MemoryStream(imageByte));
            image.Aspect = Aspect.AspectFit;
            grid.Children.Add(image, 0, 0);
            Grid.SetColumnSpan(image, 2);

            Button removeButton = new Button();
            removeButton.Text = "Remove";
            grid.Children.Add(removeButton, 0, 1);
            removeButton.Clicked += (sender, e) => {
                File.Delete(appImage.photoThumbnailFileLocation);
                if (!string.IsNullOrEmpty(appImage.photoScaledFileLocation))
                {
                    File.Delete(appImage.photoScaledFileLocation);
                }               
                actualImageFile.DeleteAsync();
                App.Database.DeleteJobImage(appImage);
                Ultis.Settings.DeleteImage = "Yes";
                Navigation.PopAsync();
            };
            Button backButton = new Button();
            backButton.Text = "Back";
            backButton.Clicked += (object sender, EventArgs e) => {
                Navigation.PopAsync();
            };
            grid.Children.Add(backButton, 1, 1);

            Content = grid;
        }
    }
}
