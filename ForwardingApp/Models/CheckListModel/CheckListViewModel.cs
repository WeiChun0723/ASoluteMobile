using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Models.CheckListModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ASolute_Mobile
{
    public class CheckListViewModel : PropertyChange
    {
        public static ObservableCollection<CheckListItem> chkList;

        public ObservableCollection<CheckListItem> Check       
        {
            set { SetProperty(ref chkList, value); }
            get { return chkList; }
        }

        public CheckListViewModel(List<clsKeyValue> listItems)
        {


            Check = new ObservableCollection<CheckListItem>();
                
            foreach (clsKeyValue item in listItems)
             {

                Check.Add( AddNew(item.Key, item.Value, false));
            };
           
        }

        public static ObservableCollection<CheckListItem> CheckedList
        {
            get { return chkList; }
        }

        private CheckListItem AddNew(string key, string value, bool isSelected)
        {
            var chkItem = new CheckListItem(key, value, isSelected);
            
            return chkItem;
        }
    }
}
