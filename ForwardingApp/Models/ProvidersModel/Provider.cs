using System;
namespace ASolute_Mobile.Models.ProvidersModel
{
    public class Provider : PropertyChange
    {
        private string code;
        private string name;
        private string url;
        private bool isSelected;

        public string Code
        {
            set { SetProperty(ref code, value); }
            get { return code; }
        }

        public string Name
        {
            set { SetProperty(ref name, value); }
            get { return name; }
        }

        public string Url
        {
            set { SetProperty(ref url, value); }
            get { return url; }
        }

        public bool IsSelected
        {
            set { SetProperty(ref isSelected, value); }
            get { return isSelected; }
        }

        public Provider(string code, string name,string url, bool isSelected)
        {
            Code = code;
            Name = name;
            Url = url;
            IsSelected = isSelected;
        }
    }
}
