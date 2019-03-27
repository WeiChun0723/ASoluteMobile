using ASolute_Mobile.Ultis;
using SQLite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xamarin.Forms;
using System.ComponentModel;

namespace ASolute_Mobile.Models
{

    public class Generic
    {      
        [PrimaryKey, AutoIncrement]
        public int tableID { get; set; }
        public string owner { get; set; }
        public DateTime? updatedDate { get; set; }      
    }

    /*public class TallyIn
    {
        public string Id { get; set; }
        public string DocumentNo { get; set; }
        public string ContainerNo { get; set; }
        public string Principal { get; set; }
    }*/

    public class ChatRecord : Generic
    {
        public string Content { get; set; }
        public string Sender { get; set; }
        public string BackgroundColor { get; set; }
    }


    public class ProviderInfo : Generic
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
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

        //testing
        public string StopId { get; set; }
        public string StopName { get; set; }
        public double Rate { get; set; }
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

    public class ListItems : Generic
    {
        public string menuId { get; set; }
        public string name { get; set; }
        public string booking { get; set; }
        public string customerRef { get; set; }
        public string category { get; set; }
        public string summary { get; set; }
        public string background { get; set; }
        public string action { get; set; }

        //testing
        public string StopId { get; set; }
        public string StopName { get; set; }
        public double Rate { get; set; }

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
        public string Summary { get; set; }

    }

    public class Log : Generic
    {
        public string logId { get; set; }
        public int Done { get; set; }
        public int OrderNo { get; set; }
    }

    public class JobNoList : Generic
    {
        public string menuId { get; set; }
        public string JobId { get; set; }
        public string JobNoValue { get; set; }
        public bool Uploaded { get; set; }
    }



    public class TruckModel : Generic
    {
        public string TruckId { get; set; }
        public string TelNo { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool ReqSign { get; set; }
        public long EventRecordId { get; set; }
        public string Title { get; set; }
        public string RefNo { get; set; }
        public string ReasonCode { get; set; }
        public string Remark { get; set; }
        public string RecordId { get; set; }
        public string Summary { get; set; }
        public string JobNo { get; set; }
        public string BackColor { get; set; }
        public string Action { get; set; }
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
    
    public class AutoComplete : Generic
    {
        public string Value { get; set; }
        public string Type { get; set; }
    }

    public class HaulageVolume
    {
        public string Entity { get; set; }

        public string Job { get; set; }

        public string Revenue { get; set; }

    }
}
