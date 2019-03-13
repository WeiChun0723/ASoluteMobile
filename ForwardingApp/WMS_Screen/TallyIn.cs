using System;
namespace ASolute_Mobile.WMS_Screen
{
	public class TallyIn : ListViewCommonScreen
    {
        public TallyIn()
        {

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            search.TextChanged += Search_TextChanged;

        }

        void Search_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
        }

    }
}
