using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ASolute_Mobile.Ultis;
using Microsoft.AppCenter.Crashes;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using ASolute_Mobile.Models;

namespace ASolute_Mobile.Utils
{
    public class ControllerUtil
    {
        public ControllerUtil()
        {
        }

        public static String getBaseURL(string enterpriseName)
        {
            return String.Format("host/handshake?Enterprise={0}", enterpriseName);
        }

        #region Common url
        #endregion 

        #region Customer Tracking url

        public static String emailVerify(string email)
        {
            return String.Format("Account/VerifyEmail?Email={0}", email);
        }

        public static String getActionURL(string deviceID,string firebase)
        {
            return String.Format("Account/Login?Id={0}&FirebaseId={1}", deviceID,firebase);
        }

        public static String getCompanyNameURL(string RegNo)
        {
            return String.Format("Account/GetCompanyName?RegNo={0}", RegNo);
        }

        public static String postRegisterURL()
        {
            return String.Format("Account/Register");
        }

        public static String sendActivationURL( string activationCode)
        {
            return String.Format("Account/Activate?Id={0}&ActivationCode={1}", Ultis.Settings.DeviceUniqueID, activationCode);
        }

        public static String sendActivationURL()
        {
            return String.Format("Account/Resend?Id={0}", Ultis.Settings.DeviceUniqueID);
        }

        public static String getAutoScan()
        {
            return String.Format("Providers/AutoScan?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getProviderList()
        {
            return String.Format("Providers/List?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getAvailableProvider()
        {
            return String.Format("Providers/New?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String saveProvider()
        {
            return String.Format("Providers/Save?SessionId={0}",Ultis.Settings.SessionSettingKey);
        }

        public static String deleteSavedProvider(string code)
        {
            return String.Format("Providers/Delete?SessionId={0}&Code={1}", Ultis.Settings.SessionSettingKey,code);
        }

        public static String getCategoryList(string code)
        {
            return String.Format("Providers/ContainerSummary?SessionId={0}&Code={1}", Ultis.Settings.SessionSettingKey,code);
        }

        public static String getContainerList(string code, string category)
        {
            return String.Format("Providers/ContainerList?SessionId={0}&Code={1}&Category={2}", Ultis.Settings.SessionSettingKey, code, category);
        }

        public static String getContainerDetail(string code, string container)
        {
            return String.Format("Providers/ContainerDetail?SessionId={0}&Code={1}&ContainerId={2}", Ultis.Settings.SessionSettingKey, code, container);
        }

        #endregion

        #region Trucking url
        #endregion

        #region fleet url
        #endregion

        #region haulage url
        #endregion

        public static String postNewRecordURL()
        {
            return String.Format("FuelCost/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String postCheckList(bool status, string remark, int odo)
        {
            return String.Format("CheckList/Save?SessionId={0}&GeoLoc={1}&IsGood={2}&Remarks={3}&Odometer={4}", Ultis.Settings.SessionSettingKey, getPositionAsync(),status,remark,odo);
        }

        public static String postNewLogRecordURL()
        {
            return String.Format("Trip/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }


        public static String postNewCargoRecordURL(string recordID)
        {
            return String.Format("Cargo/Save?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(),recordID);
        }

        
        public static String postFutileTripURL()
        {
            return String.Format("Trucking/Futile?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String postFowardJobURL(JobItems jobItem)
        {
            String remarks = jobItem.Remark;
            if (!String.IsNullOrEmpty(remarks))
            {
                remarks = System.Net.WebUtility.UrlEncode(jobItem.Remark).Replace("+", "%20");
            }
            return String.Format("Fwd/Save?SessionId={0}&GeoLoc={1}&JobId={2}&DateTime={3}&Remarks={4}", Ultis.Settings.SessionSettingKey, getPositionAsync(),jobItem.Id, System.Net.WebUtility.UrlEncode(DateTime.Now.ToString("o")), remarks);
        }

        public static String postCargoReturnURL()
        {
            return String.Format("Trucking/CargoReturn?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String postJobDetailURL()
        {
            return String.Format("Trucking/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getLoginURL(string encryptedUserId, string encryptedPassword,string equipmentId)
        {
            return String.Format("Session/Login?UserId={0}&Pwd={1}&Eq={2}", encryptedUserId, encryptedPassword,equipmentId);
        }

        public static String getRegistrationURL(string ownerID, string userID, string password, string ICNo)
        {
            return String.Format("Util/Register?GeoLoc={0}&OwnerId={1}&UserId={2}&Password={3}&ICNo={4}", "0", ownerID, userID, password, ICNo);
        }

        public static String getLoginURL(string encryptedUserId, string encryptedPassword)
        {
            return String.Format("Session/Login?UserId={0}&Pwd={1}", encryptedUserId, encryptedPassword);
        }

        public static String getDownloadLogoURL()
        {
            return String.Format("File/DownloadLogo?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getGPSTracking(string coordinate)
        {
            return String.Format("Util/GpsTracking?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey,coordinate);
        }

        public static String getDownloadLogoAcknowledgementURL(){
            return String.Format("File/LogoUpdated?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getDownloadMenuURL()
        {
            return String.Format("Session/Load?Id={0}", Ultis.Settings.SessionSettingKey);
        }

        

        public static String getDownloadRefuelHistoryURL()
        {
            return String.Format("FuelCost/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, "0");
        }

        public static String getDownloadPendingRecURL()
        {
            return String.Format("Cargo/InboundList?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getDownloadPendingLoadURL()
        {
            return String.Format("Cargo/OutboundList?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getDownloadTruckListURL()
        {
            return String.Format("Trucking/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getLocationURL(string location)
        {
            return String.Format("Util/CurrentLoc?SessionId={0}&GeoLoc={1}&LocationName={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(),location);
        }

        public static String getDownloadHaulageListURL()
        {
            return String.Format("Haulage/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, "0");
        }

        public static String getDownloadHaulageHistoryURL(string date)
        {
            return String.Format("Haulage/History?SessionId={0}&GeoLoc={1}&ViewDate={2}", Ultis.Settings.SessionSettingKey, "0",date);
        }

        public static String postContainerNumberURL(string containerNumber)
        {
            return String.Format("Haulage/Shunting?SessionId={0}&GeoLoc={1}&ContainerNo={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), containerNumber);
        }

        public static String getPendingCollectionURL()
        {
            return String.Format("Haulage/Collection?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, "0");
        }

        public static String postHaulageURL()
        {
            return String.Format("Haulage/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String addJobURL(string container)
        {
            return String.Format("haulage/plan?SessionId={0}&GeoLoc={1}&ContainerId={2}", Ultis.Settings.SessionSettingKey,getPositionAsync(),container);
        }

        public static String getDownloadFowardListURL()
        {
            return String.Format("Fwd/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getNewFuelCostURL()
        {
            return String.Format("FuelCost/New?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, "0");
        }

        public static String getReasonListURL()
        {
            return String.Format("Trucking/ReasonList?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, "0");
        }

        public static String getUploadImageURL(string LinkId)
        {
            return String.Format("File/Upload?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), LinkId);
        }

        public static String getUploadedImageURL(string Id)
        {
            return String.Format("File/List?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(),Id);
        }

        public static String getLogOutURL()
        {
            return String.Format("Session/LogOff?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getLogHistoryURL(string date)
        {
            return String.Format("Trip/List?SessionId={0}&GeoLoc={1}&ViewDate={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), date);
        }

        public static String getNewLogURL()
        {
            return String.Format("Trip/New?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getLogInfoURL( string id)
        {
            return String.Format("Trip/Load?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), id);
        }

        public static String getEquipmentURL(string eqID)
        {
            return String.Format("Inquiry/EQ?SessionId={0}&GeoLoc={1}&Eq={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), eqID);
        }

        public static String getChangePasswordURL(string encryptedNewPassword)
        {
            return String.Format("Util/ChangePwd?SessionId={0}&GeoLoc={1}&NewPwd={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(),encryptedNewPassword);
        }

        public static String getPanicURL()
        {
            return String.Format("Util/Panic?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getCallOperatorURL()
        {
            return String.Format("Util/CallMe?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getLanguageURL(int language_value)
        {
            return String.Format("Util/ChangeLanguage?SessionId={0}&GeoLoc={1}&Language={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), language_value);
        }


        public static String getPositionAsync()
        {
           
                var locator = CrossGeolocator.Current;

                if(locator != null && locator.IsGeolocationEnabled)
                {
                    if(App.gpsLocationLat == 0 || App.gpsLocationLong == 0){
                        Position position = null;
                        Boolean latestPosition = false;
                        if (locator != null && locator.IsGeolocationEnabled)
                        {
                            try
                            {
                                Task.Run(async () => { position = await locator.GetPositionAsync(TimeSpan.FromSeconds(5)); }).Wait();
                                latestPosition = true;
                            }
                            catch (TaskCanceledException exception)
                            {
                                Debug.WriteLine(exception);
                                Crashes.TrackError(exception);
                            }
                        }

                        if (position == null && !latestPosition)
                        {
                            //Task.Run(async () => { position = await locator.GetLastKnownLocationAsync(); }).Wait();
                            return "0";
                        }

                        if (position != null)
                        {
                            App.gpsLocationLat = position.Latitude;
                            App.gpsLocationLong = position.Longitude;
                        }
                    }

                    return App.gpsLocationLat.ToString() + "," + App.gpsLocationLong.ToString();
                }
            
            return "0";
        }

        
    }
}
