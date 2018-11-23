using ASolute_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASolute_Mobile.Models
{
    public class CheckListItem : PropertyChange
    {
        private string category;
        private string name;
        private bool isSelected;

        public string Category
        {
            set { SetProperty(ref category, value); }
            get { return category; }
        }

        public string Name
        {
            set { SetProperty(ref name, value); }
            get { return name; }
        }

        public bool IsSelected
        {
            set { SetProperty(ref isSelected, value); }
            get { return isSelected; }
        }

        public CheckListItem(string category, string name, bool isSelected)
        {
            Category = category;
            Name = name;            
            IsSelected = isSelected;
        }
    }
}
