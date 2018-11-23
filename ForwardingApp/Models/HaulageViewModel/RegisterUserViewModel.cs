using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ASolute_Mobile.Models.HaulageViewModel
{
    public class RegisterUserViewModel : PropertyChange
    {
        string enterpriseName;
        string userID;
        string password;
        string IC;
        bool isBusy = false;
        Command Register { get; }

        public RegisterUserViewModel()
        {
            Register = new Command(async () => await RegisterUser(),
                                            () => !IsBusy);
        }

        public string EnterpriseName
        {
            set { SetProperty(ref enterpriseName, value); }
            get {
                if (!(String.IsNullOrWhiteSpace(Ultis.Settings.AppEnterpriseName)))
                {
                    return enterpriseName = Ultis.Settings.AppEnterpriseName;
                }
                else
                {
                    return enterpriseName;
                }
            }
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

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;

                SetProperty(ref isBusy, value);

                OnPropertyChanged(nameof(IsBusy));
                Register.ChangeCanExecute();

            }
        }

        async Task RegisterUser()
        {
            IsBusy = true;



            IsBusy = false;
        }

    }
}
