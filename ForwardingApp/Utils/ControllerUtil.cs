using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ASolute_Mobile.Ultis;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using ASolute_Mobile.Models;
using Plugin.DeviceInfo;

namespace ASolute_Mobile.Utils
{
    public class ControllerUtil
    {
        public ControllerUtil()
        {
        }

        #region Common url

        public static String getBaseURL(string enterpriseName)
        {
            return String.Format("host/handshake?Enterprise={0}", enterpriseName);
        }

        public static String postNewRecordURL()
        {
            return String.Format("FuelCost/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String postCheckList(bool status, string remark, int odo)
        {
            return String.Format("CheckList/Save?SessionId={0}&GeoLoc={1}&IsGood={2}&Remarks={3}&Odometer={4}", Ultis.Settings.SessionSettingKey, getPositionAsync(), status, remark, odo);
        }

        public static String getChangePasswordURL(string encryptedNewPassword)
        {
            return String.Format("Util/ChangePwd?SessionId={0}&GeoLoc={1}&NewPwd={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), encryptedNewPassword);
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
            return String.Format("Util/ChangeLanguage?SessionId={0}&GeoLoc={1}&Language={2}", Ultis.Settings.SessionSettingKey, "", language_value);
        }
        #endregion 

        #region Customer Tracking url

        public static String emailVerify(string email)
        {
            return String.Format("Account/VerifyEmail?Email={0}", email);
        }

        public static String getActionURL(string deviceID,string firebase)
        {
            return String.Format("Account/Login?Id={0}&FirebaseId={1}&AppVer={2}", deviceID,firebase,CrossDeviceInfo.Current.AppVersion);
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

        public static String updateContainerRFC(string code, string value, string time, string remark)
        {
            return String.Format("Providers/UpdateRFC?SessionId={0}&Code={1}&ProcessId={2}&RequiredTime={3}&Remarks={4}", Ultis.Settings.SessionSettingKey, code, value, time, remark);
        }

        public static String getHaulageVolume(string dateTime)
        {
            return String.Format("Providers/HaulageVolume?SessionId={0}&ViewDate={1}", Ultis.Settings.SessionSettingKey, dateTime);
        }

        #endregion

        #region Trucking url
        public static String getPendingReceivingURL()
        {
            return String.Format("Cargo/InboundList?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getSubJobURL(string masterID)
        {
            return String.Format("Cargo/InboundList?SessionId={0}&GeoLoc={1}&MasterJobId={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), masterID);
        }

        public static String getPendingLoadURL()
        {
            return String.Format("Cargo/OutboundList?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getTruckListURL(string action)
        {
            return String.Format(action + "?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String UploadImageURL(string LinkId)
        {
            return String.Format("File/Upload?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), LinkId);
        }

        public static String postJobDetailURL(string action)
        {
            return String.Format(action + "?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey,getPositionAsync());
        }

        public static String getReasonListURL()
        {
            return String.Format("Trucking/ReasonList?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String postFutileTripURL()
        {
            return String.Format("Trucking/Futile?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getUOMListURL()
        {
            return String.Format("Cargo/Uom?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String postCargoListURL()
        {
            return String.Format("Cargo/Add?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey,getPositionAsync());
        }

        public static String postNewCargoRecordURL(string recordID)
        {
            return String.Format("Cargo/Save?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), recordID);
        }
        #endregion

        #region fleet url
        public static String postNewLogRecordURL()
        {
            return String.Format("Trip/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        #endregion

        #region haulage url

        public static String getConsigmentNote(string jobID)
        {
            return String.Format("Haulage/Print?SessionId={0}&GeoLoc={1}&id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), jobID);
        }


        public static String postDriverRFCURL(string containerNum)
        {
            return String.Format("Haulage/DriverRFC?SessionId={0}&GeoLoc={1}&ContainerNo={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), containerNum);
        }
        #endregion


        #region planner url
        public static String getEqCategory()
        {
            return String.Format("Equipment/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getEqList(string category)
        {
            return String.Format("Equipment/List?SessionId={0}&GeoLoc={1}&ViewType={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), category);
        }

        public static String getEqDetail(string equipment)
        {
            return String.Format("Equipment/Detail?SessionId={0}&GeoLoc={1}&Eq={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), equipment);
        }
        #endregion

        #region warehouse url
        public static String getTallyInList()
        {
            return String.Format("Wms/TallyIn/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String loadTallyInDetail(string tallyID)
        {
            return String.Format("Wms/TallyIn/Load?SessionId={0}&GeoLoc={1}&id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(),tallyID);
        }

        public static String loadNewPallet(string tallyID,string productLinkId)
        {
            return String.Format("Wms/TallyIn/NewPallet?SessionId={0}&GeoLoc={1}&id={2}&ProductLinkId={3}", Ultis.Settings.SessionSettingKey, getPositionAsync(), tallyID, productLinkId);
        }

        public static String postNewPallet(string tallyID)
        {
            return String.Format("Wms/TallyIn/AddPallet?SessionId={0}&GeoLoc={1}&id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), tallyID);
        }

        public static String getNewPalletTrx(string palletID)
        {
            return String.Format("Wms/Pallet/Get?SessionId={0}&GeoLoc={1}&PalletId={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), palletID);
        }

        #endregion

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

        public static String getFleetLoginURL(string encryptedUserId, string encryptedPassword,string equipmentId)
        {
            return String.Format("Session/Login?UserId={0}&Pwd={1}&Eq={2}", encryptedUserId, encryptedPassword,equipmentId);
        }

        public static String getRegistrationURL(string ownerID, string userID, string password, string ICNo)
        {
            return String.Format("Util/Register?GeoLoc={0}&OwnerId={1}&UserId={2}&Password={3}&ICNo={4}", getPositionAsync(), ownerID, userID, password, ICNo);
        }

        public static String getLoginURL(string encryptedUserId, string encryptedPassword)
        {
            return String.Format("Session/Login?UserId={0}&Pwd={1}", encryptedUserId, encryptedPassword);
        }

        public static String getDownloadLogoURL()
        {
            return String.Format("File/DownloadLogo?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getGPSTracking(string coordinate,string address)
        {
            return String.Format("Util/GpsTracking?SessionId={0}&GeoLoc={1}&LocationName={2}", Ultis.Settings.SessionSettingKey,coordinate,address);
        }

        public static String getDownloadLogoAcknowledgementURL(){
            return String.Format("File/LogoUpdated?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getDownloadMenuURL(string firebase)
        {
            return String.Format("Session/Load?Id={0}&FirebaseId={1}", Ultis.Settings.SessionSettingKey,firebase);
        }

        public static String getDownloadMenuURL()
        {
            return String.Format("Session/Load?Id={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getDownloadRefuelHistoryURL()
        {
            return String.Format("FuelCost/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }


        public static String getLocationURL(string location)
        {
            return String.Format("Util/CurrentLoc?SessionId={0}&GeoLoc={1}&LocationName={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(),location);
        }

        public static String getDownloadHaulageListURL()
        {
            return String.Format("Haulage/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getDownloadHaulageHistoryURL(string date)
        {
            return String.Format("Haulage/History?SessionId={0}&GeoLoc={1}&ViewDate={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(),date);
        }

        public static String postContainerNumberURL(string containerNumber)
        {
            return String.Format("Haulage/Shunting?SessionId={0}&GeoLoc={1}&ContainerNo={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), containerNumber);
        }

        public static String getPendingCollectionURL()
        {
            return String.Format("Haulage/Collection?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
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
            return String.Format("FuelCost/New?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
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
                                Task.Run(async () => { position = await locator.GetPositionAsync(TimeSpan.FromSeconds(15));});
                                latestPosition = true;
                            }
                            catch (TaskCanceledException exception)
                            {
                                Debug.WriteLine(exception);
                            }
                        }

                        if (position == null )
                        {
                            Task.Run(async () => { position = await locator.GetLastKnownLocationAsync(); }).Wait();
                            
                        }

                        if (position != null)
                        {
                            App.gpsLocationLat = position.Latitude;
                            App.gpsLocationLong = position.Longitude;
                        }
                     
                    }

                    return App.gpsLocationLat.ToString() + "," + App.gpsLocationLong.ToString();
                }
            
            return "";
        }

        
    }
}
