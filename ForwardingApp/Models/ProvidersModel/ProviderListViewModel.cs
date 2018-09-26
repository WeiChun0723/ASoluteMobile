using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;

namespace ASolute_Mobile.Models.ProvidersModel
{
    public class ProviderListViewModel : PropertyChange
    {
        public static ObservableCollection<Provider> providers;


        public ObservableCollection<Provider> Check
        {
            set { SetProperty(ref providers, value); }
            get { return providers; }
        }

        public ProviderListViewModel(List<clsProvider> listProviders)
        {


            Check = new ObservableCollection<Provider>();

            foreach (clsProvider item in listProviders)
            {

                Check.Add(AddNew(item.Code, item.Name, item.Url, false));
            };

        }

        public static ObservableCollection<Provider> CheckedList
        {
            get { return providers; }
        }

        private Provider AddNew(string code, string name, string url, bool isSelected)
        {
            var chkItem = new Provider(code, name, url, isSelected);

            return chkItem;
        }
    }
}
