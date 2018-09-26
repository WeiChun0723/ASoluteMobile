using System;
using System.Threading.Tasks;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;
using ASolute_Mobile.Models;

namespace ASolute_Mobile.Ultis
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants
   
        private static UserItem CurrentUserItem;

        private static readonly string SettingsDefault = string.Empty;
        private const string FirstInstall = "first";
        private const string EnterpriseName = "enterpriseName";
        private const string SessionKey = "session_key";
        private const string UserId = "session_userId";
        private const string Password = "session_password";
        private const string CurrentJobId = "job_id";
        private const string CurrentURI = "current_uri";        
        private const string SecretKey = "secret_key";
        private const string AppID = "app_id";
        private const string LoginCrash = "login_crash";
        private const string menuAction = "menuAction";
        private const string listType = "listType";
        private const string actionID = "actionID";
        private const string deleted = "No";  
        private const string languageChose = "English";
        private const string AppVersion = "";
        private const string previousOdo = "";
        private const string contextMenuTitle = "";
        private const string UpdateRecord = "Yes";
        private const string DeviceID = "";

        #endregion


        public static string DeviceUniqueID
        {
            get
            {
                return AppSettings.GetValueOrDefault(DeviceID, "");
            }
            set
            {
                AppSettings.AddOrUpdateValue(DeviceID, value);
            }
        }

        public static string UpdatedRecord
        {
            get
            {
                return AppSettings.GetValueOrDefault(UpdateRecord, "Yes");
            }
            set
            {
                AppSettings.AddOrUpdateValue(UpdateRecord, value);
            }
        }

        public static string AppFirstInstall
        {
            get
            {
                return AppSettings.GetValueOrDefault(FirstInstall, "First");
            }
            set
            {
                AppSettings.AddOrUpdateValue(FirstInstall, value);
            }
        }

        public static string AppEnterpriseName
        {
            get
            {
                return AppSettings.GetValueOrDefault(EnterpriseName, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(EnterpriseName, value);
            }
        }


        public static string SessionSettingKey
        {
            get
            {
                return AppSettings.GetValueOrDefault(SessionKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SessionKey, value);
            }
        }

        public static string SessionUserId
        {
            get
            {
                return AppSettings.GetValueOrDefault(UserId, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(UserId, value);
            }
        }

        public static string SessionPassword
        {
            get
            {
                return AppSettings.GetValueOrDefault(Password, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(Password, value);
            }
        }




        public static string SessionCurrentJobId
        {
            get
            {
                return AppSettings.GetValueOrDefault(CurrentJobId, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CurrentJobId, value);
            }
        }

        public static string SessionBaseURI
        {
            get
            {
                return AppSettings.GetValueOrDefault(CurrentURI, "https://mobile.asolute.com/devmobile/api/");
            }
            set
            {
                AppSettings.AddOrUpdateValue(CurrentURI, value);
            }
        }
      
        public static string App
        {
            get
            {
                return AppSettings.GetValueOrDefault(AppVersion, "");
            }
            set
            {
                AppSettings.AddOrUpdateValue(AppVersion, value);
            }
        }

        public static string AppSecretKey
        {
            get
            {
                return AppSettings.GetValueOrDefault(SecretKey, "Mobile@ASolute");
            }
            set
            {
                AppSettings.AddOrUpdateValue(SecretKey, value);

            }
        }    

        public static string Language
        {
            get
            {
                return AppSettings.GetValueOrDefault(languageChose, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(languageChose, value);

            }
        }


        public static string ActionID
        {
            get
            {
                return AppSettings.GetValueOrDefault(actionID, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(actionID, value);
            }
        }

        public static string ContextMenuTitle
        {
            get
            {
                return AppSettings.GetValueOrDefault(contextMenuTitle, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(contextMenuTitle, value);
            }
        }

        public static string logOdometer
        {
            get
            {
                return AppSettings.GetValueOrDefault(previousOdo, "1");
            }
            set
            {
                AppSettings.AddOrUpdateValue(previousOdo, value);
            }
        }

        public static string deleteImage
        {
            get
            {
                return AppSettings.GetValueOrDefault(deleted, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(deleted, value);
            }
        }
    

        public static string GeneratedAppID
        {
            get
            {
                return AppSettings.GetValueOrDefault(AppID, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AppID, value);
            }
        }

        public static string CrashInLoginPage
        {
            get
            {
                return AppSettings.GetValueOrDefault(LoginCrash, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(LoginCrash, value);
            }
        }

        public static string MenuAction
        {
            get
            {
                return AppSettings.GetValueOrDefault(menuAction, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(menuAction, value);
            }
        }

        public static string ListType
        {
            get
            {
                return AppSettings.GetValueOrDefault(listType, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(listType, value);
            }
        }

        public static UserItem SessionUserItem
        {
            get
            {
                if (CurrentUserItem == null)
                {
                    CurrentUserItem = ASolute_Mobile.App.Database.GetUserItemAsync();
                }
                return CurrentUserItem;
            }
            set
            {
                if (value != null)
                {
                    ASolute_Mobile.App.Database.SaveUserItem(value);
                    CurrentUserItem = ASolute_Mobile.App.Database.GetUserItemAsync();
                }
                else
                {
                    ASolute_Mobile.App.Database.DeleteUserItem();
                }

            }
        }

        public static string GetAppLogoFileLocation()
		{
			return System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/logo";
		}

        public static string GetAppLogoFileLocation(string username){
            return System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/" + username;
        }

    }
}
