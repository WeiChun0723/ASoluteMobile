using ASolute_Mobile.Models;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AppActivity : ContentPage
	{
        public MainMenu previousPage; 

		public AppActivity ()
		{
			InitializeComponent ();

            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Activity";
            }
            else
            {
               Title = "Aktiviti";
            }
            

            ObservableCollection<ActivityLog> activity = new ObservableCollection<ActivityLog>(App.Database.GetActivitiesLog());          
            ActivityList.ItemsSource = activity;          
            ActivityList.Style = (Style)App.Current.Resources["recordListStyle"];   
        }
    }
}