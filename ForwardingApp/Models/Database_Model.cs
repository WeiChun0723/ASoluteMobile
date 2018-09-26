using ASolute_Mobile.Ultis;
using SQLite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ASolute_Mobile.Models
{

    public class Generic
    {      
        [PrimaryKey, AutoIncrement]
        public int tableID { get; set; }
        public string owner { get; set; }
        public DateTime? updatedDate { get; set; }      
    }

    public class ProviderInfo : Generic
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    } 

    public class ActivityLog : Generic
    {
        public string activity { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }

    public class RefuelHistoryData : Generic
    {
        public string recordId { get; set; }
        public int Done { get; set; }  
    }

    public class SummaryItems : Generic
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public string Value { get; set; }
        public bool Display { get; set; }
        public string Type { get; set; }
        public string BackColor { get; set; }
    }

    public class DetailItems : Generic
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public string Value { get; set; }
        public bool Display { get; set; }    
    }

    public class AppImage : Generic
    {
        public string id { get; set; }
        public byte[] imageData { get; set; }
        public string photoFileLocation { get; set; }
        public string photoFileName { get; set; }
        public string photoThumbnailFileLocation { get; set; }
        public string photoScaledFileLocation { get; set; }
        public int scaleResolution { get; set; }
        public bool Uploaded { get; set; }
        public string type { get; set; }
        
    }

    public class AppMenu : Generic
    {
        public string menuId { get; set; }
    }

    public class Log : Generic
    {
        public string logId { get; set; }
        public int Done { get; set; }
        public int OrderNo { get; set; }
    }

    public class JobNoList : Generic
    {
        public string JobId { get; set; }
        public string JobNoValue { get; set; }
        public bool Uploaded { get; set; }
    }

    public class JobItems : Generic
    {
        public string TrailerId { get; set; }
        public string ContainerNo { get; set; }
        public float MaxGrossWeight { get; set; }
        public float TareWeight { get; set; }
        public bool CollectSeal { get; set; }
        public string SealNo { get; set; }
        public string ActionId { get; set; }
        public string ActionMessage { get; set; }
        public string Title { get; set; }
        public string TruckId { get; set; }
        public string Id { get; set; }
        public string telNo { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool ReqSign { get; set; }
        public long EventRecordId { get; set; }
        public int Done { get; set; }
        public string Remark { get; set; }
        public string jobNo { get; set; }
        public string RefNo { get; set; }
        public string ReasonCode { get; set; }
        public string UpdateType { get; set; }
        public string JobType { get; set; }
        public string SealMode { get; set; }

    }


    public class pickerValue : Generic
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string pickerType { get; set; }
    }

    public class FuelCostNew : Generic
    {
        public DateTime RefuelDateTime { get; set; }
        public int PreviousOdometer { get; set; }
        public double CostRate { get; set; }
        public string FuelCardNo { get; set; }     
    }

    public class RefuelData : Generic
    {
        public string ID { get; set; }
        public int Done { get; set; }
        public string TruckId { get; set; }
        public double CostRate { get; set; }
        public string DriverId { get; set; }
        public string VendorCode { get; set; }
        public DateTime RefuelDateTime { get; set; }
        public int PaymentMode { get; set; }
        public double Quantity { get; set; }
        public int Odometer { get; set; }
        public string FuelCardNo { get; set; }
        public string VoucherNo { get; set; }
        public string OtherRef { get; set; }
    }

    public class LogBookData : Generic
    {
        public string OfflineID { get; set; }
        public int Done { get; set; }
        public string LinkId { get; set; }
        public string EndGeoLoc { get; set; }
        public string EndLocationName { get; set; }
        public int EndOdometer { get; set; }       
        public DateTime? EndTime { get; set; }
        public string StartLocationName { get; set; }
        public int? StartOdometer { get; set; }
        public DateTime StartTime { get; set; }
        public string DriverId { get; set; }
        public string TruckId { get; set; }
        public string Id { get; set; }
        public string StartGeoLoc { get; set; }
    }
    
    public class AutoComplete : Generic
    {
        public string Value { get; set; }
        public string Type { get; set; }
    }

    //model for courier app
    public class UserDetail : Generic
    {
        public string OrderID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int PostCode { get; set; }
        public string CityState { get; set; }
        public int Contact { get; set; }
        public string Remarks { get; set; }
        public string OrderType { get; set; }
        public DateTime OrderDate { get; set; }
        public Double Weight { get; set; }
    }
}
