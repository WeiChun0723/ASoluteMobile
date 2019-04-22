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
    }

    public class ChatRecord : Generic
    {
        public string Content { get; set; }
        public string Sender { get; set; }
        public string BackgroundColor { get; set; }
    }*/

    public class ProviderInfo : Generic
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    } 

    public class SummaryItems : Generic
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public string Value { get; set; }
        public bool Display { get; set; }
        public string Type { get; set; }
        public string BackColor { get; set; }

        //Bus property
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
        public string Id { get; set; }
        public string Name { get; set; }
        public string Booking { get; set; }
        public string CustomerRef { get; set; }
        public string Category { get; set; }
        public string Summary { get; set; }
        public string Background { get; set; }
        public string Action { get; set; }

        //Yard
        public DateTime? ClosingDate { get; set; }

        //Bus property
        public string StopId { get; set; }
        public string StopName { get; set; }
        public double Rate { get; set; }

        //Haulage property
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
        public string TelNo { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool ReqSign { get; set; }
        public long EventRecordId { get; set; }
        public string Remark { get; set; }
        public string RefNo { get; set; }
        public string ReasonCode { get; set; }
        public string SealMode { get; set; }
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

    public class clsYardBlock
    {
        public string Id { get; set; }
        public int TotalBay { get; set; }
        public int TotalLevel { get; set; }
    }
}
