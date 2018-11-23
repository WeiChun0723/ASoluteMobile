using System;
using ASolute_Mobile.Models;

namespace ASolute_Mobile.Models.HaulageViewModel.Model
{
    public class SplashScreen :PropertyChange
    {
        private string enterpriseName;

        public string EnterpriseName
        {
            set { SetProperty(ref enterpriseName, value); }
            get { return enterpriseName; }
        }

        public SplashScreen(string enterprise)
        {
            EnterpriseName = enterprise;

        }
    }
}
