using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using ASolute_Mobile.Models;
using ASolute.Mobile.Models;
using ASolute_Mobile.Ultis;

namespace ASolute_Mobile.Data
    
{
    public class Database
    {
        readonly SQLiteConnection database;

		public Database(string dbPath)
		{
            database = new SQLiteConnection(dbPath);
            database.CreateTable<ProviderInfo>();
            database.CreateTable<UserItem>();
            database.CreateTable<UserDetail>();
            database.CreateTable<RefuelData>();
            database.CreateTable<LogBookData>();
            database.CreateTable<RefuelHistoryData>();
            database.CreateTable<SummaryItems>();
            database.CreateTable<DetailItems>();
            database.CreateTable<AppMenu>();                        
            database.CreateTable<Log>();          
            database.CreateTable<AppImage>();
            database.CreateTable<JobNoList>();
            database.CreateTable<JobItems>();
            database.CreateTable<ActivityLog>();
            database.CreateTable<pickerValue>();
            database.CreateTable<FuelCostNew> ();
            database.CreateTable<AutoComplete>();
            database.CreateTable<ListObject>();
        }

		public void DropDB()
		{
            database.DropTable<UserItem>();
            database.DropTable<UserDetail>();
            database.DropTable<RefuelData>();
            database.DropTable<LogBookData>();
            database.DropTable<RefuelHistoryData>();
            database.DropTable<SummaryItems>();
            database.DropTable<DetailItems>();
            database.DropTable<AppMenu>();
            database.DropTable<Log>();           
            database.DropTable<AppImage>();               
            database.DropTable<JobNoList>();
            database.DropTable<JobItems>();
            database.DropTable<ActivityLog>();
            database.DropTable<pickerValue>();
            database.DropTable<FuelCostNew>();
            database.DropTable<ListObject>();
            database.DropTable<ProviderInfo>();
        }

        #region Customer Tracking table function

        public int SaveProvider(ProviderInfo provider)
        {
            provider.owner = Ultis.Settings.SessionUserId;
            if (provider.tableID != 0)
            {

                return database.Update(provider);
            }
            else
            {

                return database.Insert(provider);
            }
        }


        public void DeleteProvider()
        {
            database.Query<ProviderInfo>("DELETE FROM ProviderInfo");
        }

        public List<ProviderInfo> Providers(string Code)
        {
            return database.Query<ProviderInfo>("SELECT * FROM [ProviderInfo] WHERE [Code] = ?", Code);
        }

        public int DeleteMenu(AppMenu menu)
        {
            deleteProvider(menu.menuId);
           
            return database.Delete(menu);
        }

        public void deleteProvider(string id)
        {
            database.Query<ProviderInfo>("DELETE FROM [ProviderInfo] WHERE [Code] = ?", id);
        }
        #endregion

        public int SaveUserItem(UserItem userItem)
        {
            userItem.owner = Ultis.Settings.SessionUserId;
            if (userItem.tableID != 0)
            {
                userItem.updatedDate = DateTime.Now;
                return database.Update(userItem);
            }
            else
            {
                UserItem userItemFromDB = GetUserItemAsync();
                if (userItemFromDB != null)
                {
                    userItem.tableID = userItemFromDB.tableID;
                    return SaveUserItem(userItem);
                }
                
                return database.Insert(userItem);
            }
        }

        public UserItem GetUserItemAsync()
        {
            List<UserItem> userItems = database.Query<UserItem>("SELECT * FROM [UserItem]");
            if (userItems.Count > 0)
            {
                return userItems[0];
            }
            return null;
        }

        public void DeleteUserItem()
        {
            database.DropTable<UserItem>();
            database.CreateTable<UserItem>();
        }

        public List<RefuelHistoryData> GetRecordAsync()
        {
            return toList<RefuelHistoryData>(database.Table<RefuelHistoryData>());
        }

        public List<JobItems> GetPendingRecordAsync()
        {
            return toList<JobItems>(database.Table<JobItems>());
        }

        private List<T> toList<T>(TableQuery<T> tableQuery){
            List<T> list = new List<T>();
            foreach (var item in tableQuery)
            {
                list.Add(item);
            }
            return list;
        }
    
        public RefuelHistoryData GetRecordAsync(string id)
        {
            return database.Table<RefuelHistoryData>().Where(i => i.recordId == id).FirstOrDefault();
        }

        public LogBookData GetLogOfflineAsync(string id)
        {
            return database.Table<LogBookData>().Where(i => i.Id == id).FirstOrDefault();
        }

        public JobItems GetPendingRecordAsync(string id)
        {
            return database.Table<JobItems>().Where(i => i.Id == id).FirstOrDefault();
        }

        public AppMenu GetMenuRecordAsync(string id)
        {
            return database.Table<AppMenu>().Where(i => i.menuId == id).FirstOrDefault();
        }

        public Log GetLogRecordAsync(int id)
        {
            return database.Table<Log>().Where(i => i.OrderNo == id).FirstOrDefault();
        }

        public Log GetLogRecordAsync(string id)
        {
            return database.Table<Log>().Where(i => i.logId == id).FirstOrDefault();
        }

        public clsFuelCost GetHistoryAsync(long id)
        {
            return database.Table<clsFuelCost>().Where(i => i.RecordId == id).FirstOrDefault();
        }

       
        public AutoComplete GetAutoCompleteValue(string value)
        {
            return database.Table<AutoComplete>().Where(i => i.Value == value).FirstOrDefault();
        }

        public JobNoList GetJobNoAsync(string id)
        {
            return database.Table<JobNoList>().Where(i => i.JobNoValue == id).FirstOrDefault();
        }

      
        public List<JobItems> GetJobItems(int doneStatus, string type)
        {
            return database.Query<JobItems>("SELECT * FROM [JobItems] WHERE [Done] = ? AND [JobType] = ?", doneStatus, type);
        }

        public List<ListObject> GetPendingObject()
        {
            return database.Query<ListObject>("SELECT * FROM [ListObject]");
        }

        public List<JobItems> GetDoneJobItems(int doneStatus)
        {
            return database.Query<JobItems>("SELECT * FROM [JobItems] WHERE [Done] = ? ", doneStatus);
        }

        public List<LogBookData> GetTripLog(int doneStatus)
        {
            return database.Query<LogBookData>("SELECT * FROM [LogBookData] WHERE [Done] = ?", doneStatus);
        }

        public List<ActivityLog> GetActivitiesLog()
        {
            return database.Query<ActivityLog>("SELECT * FROM [ActivityLog] ORDER BY [tableID] DESC");
        }  

        public List<RefuelHistoryData> GetRecordItems()
        {
            return database.Query<RefuelHistoryData>("SELECT * FROM [RefuelHistoryData] WHERE [owner] = ?", Ultis.Settings.SessionUserId);
        }     


        public List<AppMenu> GetMainMenuItems()
        {
            return database.Query<AppMenu>("SELECT * FROM [AppMenu] WHERE [owner] = ?", Ultis.Settings.SessionUserId);
        }

        public List<Log> GetLogItems()
        {
            return database.Query<Log>("SELECT * FROM [Log] WHERE [owner] = ? ", Ultis.Settings.SessionUserId);
        }

      

        public List<AutoComplete> GetAutoCompleteValues(string type)
        {
            return database.Query<AutoComplete>("SELECT * FROM [AutoComplete] WHERE [Type] = ?", type);
        }

        public List<pickerValue> GetPickerValue(string picker)
        {
            return database.Query<pickerValue>("SELECT * FROM [pickerValue] WHERE [pickerType] = ?",picker);
        }

     

        public List<JobNoList> GetJobNo(string jobID, bool status)
        {
            return database.Query<JobNoList>("SELECT * FROM [JobNoList] WHERE [JobId] = ? AND [Uploaded] = ?", jobID, status);
        }

        public List<JobNoList> GetJobNo()
        {
            return database.Query<JobNoList>("SELECT * FROM [JobNoList] ");
        }

        public List<AppImage> GetRecordImagesAsync(string id,bool uploaded)
        {
            return database.Query<AppImage>("SELECT * FROM [AppImage] WHERE [id] = ? AND [Uploaded] = ?", id,uploaded);
        }
        
        public List<AppImage> GetUplodedRecordImagesAsync(string id, string type)
        {
            return database.Query<AppImage>("SELECT * FROM [AppImage] WHERE [id] = ? AND [type] = ?", id,type);
        }

        public List<RefuelData> PendingRefuelData()
        {
            return database.Query<RefuelData>("SELECT * FROM [RefuelData] WHERE [Done] = 0");
        }

        public List<SummaryItems> GetSummarysAsync(string id,string type)
        {
            return database.Query<SummaryItems>("SELECT * FROM [SummaryItems] WHERE [Id] = ? AND [Type] = ?", id,type);          
        }

        public List<SummaryItems> GetSummarysAsync(string type)
        {
            return database.Query<SummaryItems>("SELECT * FROM [SummaryItems] WHERE [Type] = ?", type);
        }

        public List<DetailItems> GetDetailsAsync(string id)
        {
            return database.Query<DetailItems>("SELECT * FROM [DetailItems] WHERE [Id] = ?", id);
            //return database.Table<SummaryItem>().ToListAsync();
        }

        public JobItems GetItemAsync(string id)
        {
            return database.Table<JobItems>().Where(i => i.Id == id).FirstOrDefault();
        }      

        public FuelCostNew GetFuelCostNew()
        {
            return database.Table<FuelCostNew>().FirstOrDefault();
        }


        public void deleteJobImages(string id){
            database.Query<AppImage>("DELETE FROM [Image] WHERE [jobId] = ?", id);
        }

        public void deleteJobNo()
        {
            database.Query<JobNoList>("DELETE FROM [JobNoList]");
        }

        public void deletePickerValue()
        {
            database.Query<pickerValue>("DELETE FROM [pickerValue]");
        }

        public void deleteMainMenu()
        {
            database.Query<AppMenu>("DELETE FROM AppMenu");
        }

        public void deleteHistory()
        {
            database.Query<RefuelHistoryData>("DELETE FROM RefuelHistoryData ");
        }

        
        public void deleteMenuItems(string type)
        {
            database.Query<SummaryItems>("DELETE FROM SummaryItems WHERE [type] = ?" , type);
        }
        
        public void deleteSummary(string id)
        {
            database.Query<SummaryItems>("DELETE FROM SummaryItems WHERE [id] = ?", id);
        }

        public void deleteDetail(string id)
        {
            database.Query<DetailItems>("DELETE FROM DetailItems WHERE [id] = ?",id);
        }

        public void deleteLogHistory()
        {
            database.Query<Log>("DELETE FROM Log");
        }

        public void deleteLogPendingData()
        {
            database.Query<LogBookData>("DELETE FROM LogBookData");
        }

        public void deletePending(string type)
        {
            database.Query<JobItems>("DELETE FROM JobItems WHERE [Done] = 0 AND [JobType] = ?",type);
        }

        public void deleteDoneJob(string id)
        {
            database.Query<JobItems>("DELETE FROM JobItems WHERE [Id] = ?", id);
        }

        /*public void deleteHaulage()
        {
            database.Query<JobItems>("DELETE FROM JobItems ");
        }*/

        public void deleteHaulage(string type)
        {
            database.Query<JobItems>("DELETE FROM JobItems WHERE [JobType] = ?", type);
        }

        /*public void deleteHaulageSummary()
        {
            database.Query<SummaryItems>("DELETE FROM SummaryItems ");
        }*/

        public void deletePendingObject()
        {
            database.Query<ListObject>("DELETE FROM ListObject");
        }

        public void deleteHaulageSummary(string type)
        {
            database.Query<SummaryItems>("DELETE FROM SummaryItems WHERE [Type] = ?", type);
        }

        public void deleteHaulageDetail()
        {
            database.Query<DetailItems>("DELETE FROM DetailItems");
        }

        public void deleteLocationAutoComplete(string type)
        {
            database.Query<AutoComplete>("DELETE FROM AutoComplete WHERE [Type] = ?", type);
        }

        public void deleteLogData()
        {
            database.Query<LogBookData>("DELETE FROM LogBookData");
        }

        public void deleteDonePending()
        {
            database.Query<JobItems>("DELETE FROM JobItems ");
        }

        public void deleteFuelCostNew()
        {
            database.Query<JobItems>("DELETE FROM JobItems ");
        }

        public void deleteAppImage()
        {
            database.Query<AppImage>("DELETE FROM AppImage");
        }

        public int SaveActivity(ActivityLog activity)
        {
           activity.owner = Ultis.Settings.SessionUserId;
            if (activity.tableID != 0)
            {
                activity.updatedDate = DateTime.Now;
                return database.Update(activity);
            }
            else
            {
                activity.updatedDate = DateTime.Now;
                return database.Insert(activity);
            }
        }


        public int SaveFuelCostNew(FuelCostNew fuelCost)
        {
            if (fuelCost.tableID != 0)
            {

                return database.Update(fuelCost);
            }
            else
            {         
                return database.Insert(fuelCost);
            }
        }

        public int SaveHistoryAsync(RefuelHistoryData history)
        {
            history.owner = Ultis.Settings.SessionUserId;
            if (history.tableID != 0)
            {
               
                return database.Update(history);
            }
            else
            {
                
                return database.Insert(history);
            }

        }

        public int SaveUserDetail(UserDetail detail)
        {
            detail.owner = Ultis.Settings.SessionUserId;
            if (detail.tableID != 0)
            {

                return database.Update(detail);
            }
            else
            {

                return database.Insert(detail);
            }

        }

        public int SaveJobsAsync(JobItems job)
        {
            job.owner = Ultis.Settings.SessionUserId;
            if (job.tableID != 0)
            {

                return database.Update(job);
            }
            else
            {

                return database.Insert(job);
            }

        }

        public int SavePendingAsync(ListObject data)
        {
            data.owner = Ultis.Settings.SessionUserId;
            if (data.tableID != 0)
            {

                return database.Update(data);
            }
            else
            {

                return database.Insert(data);
            }

        }

        public int SaveMenuAsync(AppMenu item)
        {
            item.owner = Ultis.Settings.SessionUserId;
            if (item.tableID != 0)
            {

                return database.Update(item);
            }
            else
            {

                return database.Insert(item);
            }

        }

        public int SavePickerValue(pickerValue code)
        {
            code.owner = Ultis.Settings.SessionUserId;
            if (code.tableID != 0)
            {

                return database.Update(code);
            }
            else
            {

                return database.Insert(code);
            }

        }

        public int SaveLogAsync(Log log)
        {
            log.owner = Ultis.Settings.SessionUserId;
            if (log.tableID != 0)
            {

                return database.Update(log);
            }
            else
            {

                return database.Insert(log);
            }

        }

        public int SaveRecordImageAsync(AppImage item)
        {
            if (item.tableID != 0)
            {
                
                return database.Update(item);
            }
            else
            {
                
                return database.Insert(item);
            }
        }

       
        public int SaveSummarysAsync(SummaryItems item)
        {
            if (item.tableID != 0)
            {
               
                return database.Update(item);
            }
            else
            {
               
                return database.Insert(item);
            }
        }

        public int SaveDetailsAsync(DetailItems item)
        {
            if (item.tableID != 0)
            {

                return database.Update(item);
            }
            else
            {

                return database.Insert(item);
            }
        }

        public int SaveJobNoAsync(JobNoList item)
        {
            if (item.tableID != 0)
            {

                return database.Update(item);
            }
            else
            {
                return database.Insert(item);
            }
        }

        public int SaveRecordAsync(RefuelData refuel)
        {
            if (refuel.tableID != 0)
            {
               
                return database.Update(refuel);
            }
            else
            {
               
                return database.Insert(refuel);
            }
        }

        public int SaveLogRecordAsync(LogBookData log)
        {
            if (log.tableID != 0)
            {

                return database.Update(log);
            }
            else
            {

                return database.Insert(log);
            }
        }

    

        public int SaveAutoCompleteAsync(AutoComplete value)
        {
            if (value.tableID != 0)
            {

                return database.Update(value);
            }
            else
            {

                return database.Insert(value);
            }
        }

        public int DeleteJobImage(AppImage item)
        {
            return database.Delete(item);
        }

        public int DeleteSummaryItem(SummaryItems item){
            return database.Delete(item);
        }

        public int DeleteDetailItem(DetailItems item)
        {
            return database.Delete(item);
        }
        
    }
}
