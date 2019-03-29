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
        private const string Enterprise = "enterpriseName";
        private const string SessionKey = "session_key";
        private const string UserId = "session_userId";
        private const string Password = "session_password";
        private const string CurrentJobId = "job_id";
        private const string CurrentURI = "current_uri";        
        private const string SecretKey = "secret_key";
        private const string MenuAction = "menuAction";
        private const string ListType = "listType";
        private const string ActionID = "actionID";
        private const string Deleted = "No";  
        private const string LanguageChose = "English";
        private const string AppName = "app_Name";
        private const string ContextMenuTitle = "";
        private const string UpdateRecord = "Yes";
        private const string RefreshMenu = "refresh_menu";
        private const string DeviceID = "testing";
        private const string FirebaseID = "firebase";
        private const string UserEmail = "";
        private const string Cargo_Return = "return";
        private const string New_Job = "new_job";
        private const string SubTitles = "sub_Titles";
        private const string Titles = "title";
        private const string UpdateTimes = "update_time";
        #endregion


        public static string UpdateTime
        {
            get
            {
                return AppSettings.GetValueOrDefault(UpdateTimes, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(UpdateTimes, value);
            }
        }

        public static string Title
        {
            get
            {
                return AppSettings.GetValueOrDefault(Titles, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(Titles, value);
            }
        }

        public static string SubTitle
        {
            get
            {
                return AppSettings.GetValueOrDefault(SubTitles, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SubTitles, value);
            }
        }

        public static string NewJob
        {
            get
            {
                return AppSettings.GetValueOrDefault(New_Job, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(New_Job, value);
            }
        }


        public static string CargoReturn
        {
            get
            {
                return AppSettings.GetValueOrDefault(Cargo_Return, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(Cargo_Return, value);
            }
        }

        public static string Email
        {
            get
            {
                return AppSettings.GetValueOrDefault(UserEmail, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(UserEmail, value);
            }
        }


        public static string DeviceUniqueID
        {
            get
            {
                return AppSettings.GetValueOrDefault(DeviceID, "testing");
            }
            set
            {
                AppSettings.AddOrUpdateValue(DeviceID, value);
            }
        }

        public static string FireID
        {
            get
            {
                return AppSettings.GetValueOrDefault(FirebaseID, "firebase");
            }
            set
            {
                AppSettings.AddOrUpdateValue(FirebaseID, value);
            }
        }

        public static string UpdatedRecord
        {
            get
            {
                return AppSettings.GetValueOrDefault(UpdateRecord, "RefreshJobList");
            }
            set
            {
                AppSettings.AddOrUpdateValue(UpdateRecord, value);
            }
        }

        public static string RefreshMenuItem
        {
            get
            {
                return AppSettings.GetValueOrDefault(RefreshMenu, "Yes");
            }
            set
            {
                AppSettings.AddOrUpdateValue(RefreshMenu, value);
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

        public static string EnterpriseName
        {
            get
            {
                return AppSettings.GetValueOrDefault(Enterprise, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(Enterprise, value);
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
                return AppSettings.GetValueOrDefault(AppName, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AppName, value);
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
                return AppSettings.GetValueOrDefault(LanguageChose, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(LanguageChose, value);

            }
        }


        public static string Action
        {
            get
            {
                return AppSettings.GetValueOrDefault(ActionID, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ActionID, value);
            }
        }

        public static string ContextMenu
        {
            get
            {
                return AppSettings.GetValueOrDefault(ContextMenuTitle, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ContextMenuTitle, value);
            }
        }

        public static string DeleteImage
        {
            get
            {
                return AppSettings.GetValueOrDefault(Deleted, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(Deleted, value);
            }
        }

        public static string MenuRequireAction
        {
            get
            {
                return AppSettings.GetValueOrDefault(MenuAction, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(MenuAction, value);
            }
        }

        public static string List
        {
            get
            {
                return AppSettings.GetValueOrDefault(ListType, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ListType, value);
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
