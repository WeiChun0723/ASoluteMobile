using System;
using System.Collections.Generic;
using ASolute_Mobile.Models;
using SQLite;

namespace ASolute_Mobile.Ultis
{
    public class UserItem : Generic
    {
        public string TruckId { get; set; }
        public string ServiceId { get; set; }
        public string TrailerPrefix { get; set; }
        public string OwnerName { get; set; }
        public string OperationPhone { get; set; }
        public string SessionId { get; set; }
        public Boolean GetLogo { get; set; }
        public string UserName { get; set; }
        public string ImageSize { get; set; }
        public string MaxUploadFile { get; set; }
        public Boolean GetGPS { get; set; }
        public bool GetSignature { get; set; }
        public string CompanyName { get; set; }
        public string UserId { get; set; }
        public string DriverId { get; set; }
        public string TruckPrefix { get; set; }

    }
}

