using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Foundation;
using Tesseract.iOS;
using UIKit;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Ioc.Autofac;
using XLabs.Platform.Services.Media;

namespace ASolute_Mobile.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {

            Rg.Plugins.Popup.Popup.Init();
            global::Xamarin.Forms.Forms.Init();

            var containerBuilder = new Autofac.ContainerBuilder();

            containerBuilder.RegisterType<MediaPicker>().As<IMediaPicker>();
            containerBuilder.RegisterType<TesseractApi>().As<Tesseract.ITesseractApi>();

            Resolver.SetResolver(new AutofacResolver(containerBuilder.Build()));

            new Syncfusion.SfAutoComplete.XForms.iOS.SfAutoCompleteRenderer();
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            LoadApplication(new App());

            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);

            App.DisplayScreenWidth = UIScreen.MainScreen.Bounds.Width;

            return base.FinishedLaunching(app, options);
        }

        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            // Check for new data, and display it


            // Inform system of fetch results
            completionHandler(UIBackgroundFetchResult.NewData);
        }
    }
}
