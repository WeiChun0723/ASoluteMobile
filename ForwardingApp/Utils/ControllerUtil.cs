using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ASolute_Mobile.Ultis;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using ASolute_Mobile.Models;
using Plugin.DeviceInfo;
using Xamarin.Essentials;

namespace ASolute_Mobile.Utils
{
    public class ControllerUtil
    {
        #region AILS Yard
        public static String getPendingStorage()
        {
            return String.Format("Yard/PendingStorage?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey,getPositionAsync());
        }

        public static String getBlockList()
        {
            return String.Format("Yard/BlockList?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String confirmBlock(string id, string location)
        {
            return String.Format("Yard/ConfirmLocation?SessionId={0}&GeoLoc={1}&Id={2}&LocationId={3}", Ultis.Settings.SessionSettingKey, getPositionAsync(),id, location);
        }

        public static String getCollectionInquiry()
        {
            return String.Format("Yard/Inventory?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }
        #endregion

        #region bus ticketing url
        public static String getBusStops(string list)
        {
            return String.Format("Ticket/{0}", list);
        }

        public static String getBusTripHistory(string date)
        {
            return String.Format("Trip/History?SessionId={0}&GeoLoc={1}&ViewDate={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), date);
        }

        public static String postTrips()
        {
            return String.Format("Trip/SaveMultiple?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String postTickets()
        {
            return String.Format("Ticket/Save?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }
        #endregion

        #region Common url
        public static String getRegistrationURL(string ownerID, string userID, string password, string ICNo)
        {
            return String.Format("Util/Register?GeoLoc={0}&OwnerId={1}&UserId={2}&Password={3}&ICNo={4}", getPositionAsync(), ownerID, userID, password, ICNo);
        }

        public static String getBaseURL(string enterpriseName)
        {
            return String.Format("host/handshake?Enterprise={0}", enterpriseName);
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

        public static String getLocationURL(string location)
        {
            return String.Format("Util/CurrentLoc?SessionId={0}&GeoLoc={1}&LocationName={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), location);
        }

        public static String getDownloadLogoURL()
        {
            return String.Format("File/DownloadLogo?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getGPSTracking(string coordinate, string address)
        {
            return String.Format("Util/GpsTracking?SessionId={0}&GeoLoc={1}&LocationName={2}", Ultis.Settings.SessionSettingKey, coordinate, address);
        }

        public static String getLogOutURL()
        {
            return String.Format("Session/LogOff?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getUploadedImageURL(string Id)
        {
            return String.Format("File/List?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), Id);
        }

        //normal app
        public static String getLoginURL(string encryptedUserId, string encryptedPassword)
        {
            return String.Format("Session/Login?UserId={0}&Pwd={1}", encryptedUserId, encryptedPassword);
        }

        //fleet app
        public static String getLoginURL(string encryptedUserId, string encryptedPassword, string equipment)
        {
            return String.Format("Session/Login?UserId={0}&Pwd={1}&Eq={2}", encryptedUserId, encryptedPassword, equipment);
        }

        public static String getDownloadLogoAcknowledgementURL()
        {
            return String.Format("File/LogoUpdated?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getDownloadMenuURL()
        {
            string name = "";
            switch (Ultis.Settings.App)
            {
                case "asolute.Mobile.AILSHaulage":
                    name = "Haulage";
                    break;

                case "asolute.Mobile.AILSWMS":
                    name = "WMS";
                    break;

                case "asolute.Mobile.ASOLUTEFLEET":
                    name = "Fleet";
                    break;

                case "asolute.Mobile.AILSYard":
                    name = "Yard";
                    break;

                case "asolute.Mobile.AILSBUS":
                    name = "Bus";
                    break;
            }

            return String.Format("Session/Load?Id={0}&AppName={1}&AppVer={2}", Ultis.Settings.SessionSettingKey, name, CrossDeviceInfo.Current.AppVersion);
        }

        /* public static String getDownloadMenuURL(string firebase)
         {
             return String.Format("Session/Load?Id={0}&FirebaseId={1}&AppName={2}&AppVer={3}", Ultis.Settings.SessionSettingKey, firebase, "Haulage", CrossDeviceInfo.Current.AppVersion);
         }*/

        #endregion

        #region Customer tracking url

        public static String emailVerifyURL(string email)
        {
            return String.Format("Account/VerifyEmail?Email={0}", email);
        }

        public static String getActionURL(string deviceID, string firebase)
        {
            return String.Format("Account/Login?Id={0}&FirebaseId={1}&AppVer={2}", deviceID, firebase, CrossDeviceInfo.Current.AppVersion);
        }

        public static String getCompanyNameURL(string RegNo)
        {
            return String.Format("Account/GetCompanyName?RegNo={0}", RegNo);
        }

        public static String postBusinessRegisterURL()
        {
            return String.Format("Account/Register");
        }

        public static String sendActivationURL(string activationCode)
        {
            return String.Format("Account/Activate?Id={0}&ActivationCode={1}", Ultis.Settings.DeviceUniqueID, activationCode);
        }

        public static String sendActivationURL()
        {
            return String.Format("Account/Resend?Id={0}", Ultis.Settings.DeviceUniqueID);
        }

        public static String getAutoScanURL()
        {
            return String.Format("Providers/AutoScan?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getProviderListURL()
        {
            return String.Format("Providers/List?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String getAvailableProviderURL()
        {
            return String.Format("Providers/New?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String saveProviderURL()
        {
            return String.Format("Providers/Save?SessionId={0}", Ultis.Settings.SessionSettingKey);
        }

        public static String deleteSavedProviderURL(string code)
        {
            return String.Format("Providers/Delete?SessionId={0}&Code={1}", Ultis.Settings.SessionSettingKey, code);
        }

        public static String getCategoryListURL(string code)
        {
            return String.Format("Providers/ContainerSummary?SessionId={0}&Code={1}", Ultis.Settings.SessionSettingKey, code);
        }

        public static String getContainerListURL(string code, string category)
        {
            return String.Format("Providers/ContainerList?SessionId={0}&Code={1}&Category={2}", Ultis.Settings.SessionSettingKey, code, category);
        }

        public static String getContainerDetailURL(string code, string container)
        {
            return String.Format("Providers/ContainerDetail?SessionId={0}&Code={1}&ContainerId={2}", Ultis.Settings.SessionSettingKey, code, container);
        }

        public static String updateContainerRFCURL(string code, string value, string time, string remark)
        {
            return String.Format("Providers/UpdateRFC?SessionId={0}&Code={1}&ProcessId={2}&RequiredTime={3}&Remarks={4}", Ultis.Settings.SessionSettingKey, code, value, time, remark);
        }

        public static String getHaulageVolumeURL(string dateTime)
        {
            return String.Format("Providers/HaulageVolume?SessionId={0}&ViewDate={1}", Ultis.Settings.SessionSettingKey, dateTime);
        }

        #endregion

        #region trucking url
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
            return String.Format(action + "?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
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
            return String.Format("Cargo/Add?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
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

        public static String getLogHistoryURL(string date)
        {
            return String.Format("Trip/List?SessionId={0}&GeoLoc={1}&ViewDate={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), date);
        }

        public static String getNewLogURL()
        {
            return String.Format("Trip/New?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getLogInfoURL(string id)
        {
            return String.Format("Trip/Load?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), id);
        }


        public static String getDownloadRefuelHistoryURL()
        {
            return String.Format("FuelCost/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getNewFuelCostURL()
        {
            return String.Format("FuelCost/New?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String postNewRecordURL()
        {
            return String.Format("FuelCost/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }
        #endregion

        #region haulage url

        public static String getConsigmentNoteURL(string jobID)
        {
            return String.Format("Haulage/Print?SessionId={0}&GeoLoc={1}&id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), jobID);
        }

        public static String getHaulageJobListURL()
        {
            return String.Format("Haulage/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String postDriverRFCURL(string containerNum)
        {
            return String.Format("Haulage/DriverRFC?SessionId={0}&GeoLoc={1}&ContainerNo={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), containerNum);
        }

        public static String getPendingCollectionURL()
        {
            return String.Format("Haulage/Collection?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String updateHaulageJobURL()
        {
            return String.Format("Haulage/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String postCargoReturnURL()
        {
            return String.Format("Trucking/CargoReturn?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }



        public static String getDownloadHaulageHistoryURL(string date)
        {
            return String.Format("Haulage/History?SessionId={0}&GeoLoc={1}&ViewDate={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), date);
        }

        public static String postContainerNumberURL(string containerNumber)
        {
            return String.Format("Haulage/Shunting?SessionId={0}&GeoLoc={1}&ContainerNo={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), containerNumber);
        }


        public static String addJobURL(string container)
        {
            return String.Format("haulage/plan?SessionId={0}&GeoLoc={1}&ContainerId={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), container);
        }

        public static String getEquipmentURL(string eqID)
        {
            return String.Format("Inquiry/EQ?SessionId={0}&GeoLoc={1}&Eq={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), eqID);
        }
        #endregion

        #region planner url
        public static String getEqCategoryURL()
        {
            return String.Format("Equipment/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getEqListURL(string category)
        {
            return String.Format("Equipment/List?SessionId={0}&GeoLoc={1}&ViewType={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), category);
        }

        public static String getEqDetailURL(string equipment)
        {
            return String.Format("Equipment/Detail?SessionId={0}&GeoLoc={1}&Eq={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), equipment);
        }

        public static String getAllEqURL()
        {
            return String.Format("Equipment/ListAll?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }
        #endregion

        #region warehouse url
        public static String getTallyInListURL()
        {
            return String.Format("Wms/TallyIn/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String loadTallyInDetailURL(string tallyID)
        {
            return String.Format("Wms/TallyIn/Load?SessionId={0}&GeoLoc={1}&id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), tallyID);
        }

        public static String loadNewPalletURL(string tallyID, string productLinkId)
        {
            return String.Format("Wms/TallyIn/NewPallet?SessionId={0}&GeoLoc={1}&id={2}&ProductLinkId={3}", Ultis.Settings.SessionSettingKey, getPositionAsync(), tallyID, productLinkId);
        }

        public static String postNewPalletURL(string tallyID)
        {
            return String.Format("Wms/TallyIn/AddPallet?SessionId={0}&GeoLoc={1}&id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), tallyID);
        }

        public static String getPalletInquiryURL(string palletID)
        {
            return String.Format("Wms/Pallet/Get?SessionId={0}&GeoLoc={1}&PalletId={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), palletID);
        }

        public static String getPalletVerificationURL(string id, string palletID)
        {
            return String.Format("Wms/Picking/Verify?SessionId={0}&GeoLoc={1}&Id={2}&PalletId={3}", Ultis.Settings.SessionSettingKey, getPositionAsync(), id, palletID);
        }

        public static String postNewPalletTrxURL()
        {
            return String.Format("Wms/Pallet/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getPickingListURL(string menuId)
        {
            return String.Format("Wms/Picking/List?SessionId={0}&GeoLoc={1}&menuId={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), menuId);
        }

        public static String getPickingListURL()
        {
            return String.Format("Wms/Picking/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String loadPickingDetailURL(string pickingID)
        {
            return String.Format("Wms/Picking/Load?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), pickingID);
        }

        public static String loadPickingDetailURL(string pickingID, string pickingType)
        {
            return String.Format("Wms/Picking/Load?SessionId={0}&GeoLoc={1}&Id={2}&menuId={3}", Ultis.Settings.SessionSettingKey, getPositionAsync(), pickingID, pickingType);
        }

        public static String postPickingDetailURL()
        {
            return String.Format("Wms/Picking/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getPackingListURL()
        {
            return String.Format("Wms/Packing/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String loadPackingDetailURL(string packingID)
        {
            return String.Format("Wms/Packing/Load?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), packingID);
        }

        public static String postPackingDetailURL()
        {
            return String.Format("Wms/Packing/Save?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String getTallyOutListURL()
        {
            return String.Format("Wms/TallyOut/List?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String loadTallyOutDetailURL(string tallyOutID)
        {
            return String.Format("Wms/TallyOut/Load?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), tallyOutID);
        }

        public static String postTallyOutDetailURL()
        {
            return String.Format("Wms/TallyOut/AddPallet?SessionId={0}&GeoLoc={1}", Ultis.Settings.SessionSettingKey, getPositionAsync());
        }

        public static String generatePalletURL(string linkid)
        {
            return String.Format("Wms/Picking/GenPallet?SessionId={0}&GeoLoc={1}&Id={2}", Ultis.Settings.SessionSettingKey, getPositionAsync(), linkid);
        }

        #endregion

        #region getPosition
        public static String getPositionAsync()
        {
            var locator = CrossGeolocator.Current;

            if (locator != null && locator.IsGeolocationEnabled)
            {
                if (App.gpsLocationLat == 0 || App.gpsLocationLong == 0)
                {
                    Location location = new Location();
                    //Position position = null;
                    if (locator != null && locator.IsGeolocationEnabled)
                    {
                        try
                        {
                            //Task.Run(async () => { position = await locator.GetPositionAsync(TimeSpan.FromSeconds(15)); });
                            Task.Run(async () => { location = await Geolocation.GetLastKnownLocationAsync(); });
                        }
                        catch (TaskCanceledException exception)
                        {
                            Debug.WriteLine(exception);
                        }
                    }

                    /*if (position == null)
                    {
                        Task.Run(async () => { position = await locator.GetLastKnownLocationAsync(); });

                    }*/

                    if (location != null)
                    {
                        //App.gpsLocationLat = position.Latitude;
                        //App.gpsLocationLong = position.Longitude;

                        App.gpsLocationLat = location.Latitude;
                        App.gpsLocationLong = location.Longitude;
                    }

                }

                return App.gpsLocationLat.ToString() + "," + App.gpsLocationLong.ToString();
            }

            return "0,0";
        }
        #endregion


    }
}
