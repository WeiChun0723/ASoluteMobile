using System;
using System.IO;
using System.Text;
using ASolute_Mobile.Ultis;

namespace ASolute_Mobile.Models
{
    public class UploadFileDTO
    {

        //public string key { get; set; }
        //public string GeoLoc { get; set; }
        //public string JobId { get; set; }
        //public string FileName { get; set; }
        public string Content { get; set; }

        public UploadFileDTO()
        {
        }

        public UploadFileDTO(AppImage appImage){
            //JobId = jobImage.jobId;
            //FileName = HelperUtil.GetPhotoFileName(jobImage.photoScaledFileLocation);
            Content = System.Net.WebUtility.UrlEncode(Convert.ToBase64String(File.ReadAllBytes(appImage.photoScaledFileLocation)));
        }
    }
}
