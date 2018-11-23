using System;
namespace ASolute_Mobile.Models.HaulageViewModel.Model
{
    public class Register : PropertyChange
    {
        private string enterpriseName;
        private string userID;
        private string password;
        private string IC;

        public string EnterpriseName
        {
            set { SetProperty(ref enterpriseName, value); }
            get { return enterpriseName; }
        }

        public string UserID
        {
            set { SetProperty(ref userID, value); }
            get { return userID; }
        }

        public string Password
        {
            set { SetProperty(ref password, value); }
            get { return password; }
        }

        public string ICNumber
        {
            set { SetProperty(ref IC, value); }
            get { return IC; }
        }

        public Register(string enterpriseName, string userID, string password, string IC)
        {
            EnterpriseName = enterpriseName;
            UserID = userID;
            password = Password;
            ICNumber = IC;
        }
    }
}
