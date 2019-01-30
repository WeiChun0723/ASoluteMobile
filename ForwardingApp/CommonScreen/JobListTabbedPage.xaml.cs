
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class JobListTabbedPage : TabbedPage
	{
        public static bool fromJobDetailPage = false;

        public JobListTabbedPage ()
		{
			InitializeComponent ();
            this.BarBackgroundColor = Color.FromHex("#9A2116");
            if (Ultis.Settings.MenuRequireAction.Equals("pending_receiving"))
            {
                Title = "Pending Receiving";
            }
            else if (Ultis.Settings.MenuRequireAction.Equals("pending_loading"))
            {
                Title = "Pending Loading";
            }
            else if (Ultis.Settings.MenuRequireAction.Equals("Job_List"))
            {
                Title = "Job List";
                if (Ultis.Settings.Language.Equals("English"))
                {
                    Title = "Job List";
                }
                else 
                {
                    Title = "Senarai Kerja";
                }
            }
        }
	}
}