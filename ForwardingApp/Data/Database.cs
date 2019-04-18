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
            database.CreateTable<ChatRecord>();
            database.CreateTable<ProviderInfo>();
            database.CreateTable<UserItem>();
            database.CreateTable<SummaryItems>();
            database.CreateTable<DetailItems>();
            database.CreateTable<ListItems>();                            
            database.CreateTable<AppImage>();
            database.CreateTable<JobNoList>();
            database.CreateTable<TruckModel>();
            database.CreateTable<AutoComplete>();
        }

		public void DropDB()
		{
            database.DropTable<ChatRecord>();
            database.DropTable<UserItem>();
            database.DropTable<SummaryItems>();
            database.DropTable<DetailItems>();
            database.DropTable<ListItems>();          
            database.DropTable<AppImage>();               
            database.DropTable<JobNoList>();
            database.DropTable<TruckModel>();
            database.DropTable<ProviderInfo>();
        }

        #region get/store app image
        public List<AppImage> GetRecordImagesAsync(string id, bool uploaded)
        {
            return database.Query<AppImage>("SELECT * FROM [AppImage] WHERE [id] = ? AND [Uploaded] = ?", id, uploaded);
        }

        public List<AppImage> GetUplodedRecordImagesAsync(string id, string type)
        {
            return database.Query<AppImage>("SELECT * FROM [AppImage] WHERE [id] = ? AND [type] = ?", id, type);
        }

        public List<AppImage> GetPendingRecordImages(bool uploaded)
        {
            return database.Query<AppImage>("SELECT * FROM [AppImage] WHERE [Uploaded] = ?", uploaded);
        }

        //delete user previous profile picture
        public void DeleteUserImage(string userId)
        {
            database.Query<AppImage>("DELETE FROM [AppImage] WHERE [id] = ?", userId);
        }

        public AppImage GetUserProfilePicture(string id)
        {
            return database.Table<AppImage>().Where(i => i.id == id).FirstOrDefault();
        }

        public void deleteAppImage()
        {
            database.Query<AppImage>("DELETE FROM AppImage");
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

        public int DeleteJobImage(AppImage item)
        {
            return database.Delete(item);
        }
        #endregion

        #region ChatApp

        public int SaveChat(ChatRecord record)
        {
            record.owner = Ultis.Settings.SessionUserId;
            if (record.tableID != 0)
            {

                return database.Update(record);
            }
            else
            {

                return database.Insert(record);
            }
        }

        public List<ChatRecord> Chats()
        {
            return database.Query<ChatRecord>("SELECT * FROM [ChatRecord] WHERE [owner] = ?", Ultis.Settings.SessionUserId);
        }

        #endregion


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

        public int DeleteMenu(ListItems menu)
        {
            deleteProvider(menu.Id);
           
            return database.Delete(menu);
        }

        public void deleteProvider(string id)
        {
            database.Query<ProviderInfo>("DELETE FROM [ProviderInfo] WHERE [Code] = ?", id);
        }
        #endregion

        #region Trucking
        public int SaveTruckModelAsync(TruckModel record)
        {
            record.owner = Ultis.Settings.SessionUserId;
            if (record.tableID != 0)
            {

                return database.Update(record);
            }
            else
            {

                return database.Insert(record);
            }

        }

        public List<TruckModel> GetPendingRecord()
        {
            return database.Query<TruckModel>("SELECT * FROM [TruckModel] WHERE [owner] = ?", Ultis.Settings.SessionUserId);
        }

        public void DeleteTruckModel()
        {
            database.Query<TruckModel>("DELETE FROM TruckModel");
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

        public void DeleteTruckModeDetail()
        {
            database.Query<DetailItems>("DELETE FROM DetailItems");
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

        private List<T> toList<T>(TableQuery<T> tableQuery){
            List<T> list = new List<T>();
            foreach (var item in tableQuery)
            {
                list.Add(item);
            }
            return list;
        }

        public AutoComplete GetAutoCompleteValue(string value)
        {
            return database.Table<AutoComplete>().Where(i => i.Value == value).FirstOrDefault();
        }

        public JobNoList GetJobNoAsync(string id)
        {
            return database.Table<JobNoList>().Where(i => i.JobNoValue == id).FirstOrDefault();
        }

        public List<ListItems> GetMainMenuItems()
        {
            return database.Query<ListItems>("SELECT * FROM [ListItems] WHERE [owner] = ?", Ultis.Settings.SessionUserId);
        }

        public List<ListItems> GetMainMenu(string category)
        {
            return database.Query<ListItems>("SELECT * FROM [ListItems] WHERE [Category] = ?", category);
        }

        public List<ListItems> GetStops(string category,string stopId)
        {
            return database.Query<ListItems>("SELECT * FROM [ListItems] WHERE [Category] = ? AND [StopId] = ?", category, stopId);
        }


        public List<AutoComplete> GetAutoCompleteValues(string type)
        {
            return database.Query<AutoComplete>("SELECT * FROM [AutoComplete] WHERE [Type] = ?", type);
        }

        public List<JobNoList> GetJobNo(string jobID, bool status)
        {
            return database.Query<JobNoList>("SELECT * FROM [JobNoList] WHERE [JobId] = ? AND [Uploaded] = ?", jobID, status);
        }

        public List<JobNoList> GetJobNo()
        {
            return database.Query<JobNoList>("SELECT * FROM [JobNoList] ");
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

        public ListItems GetJobRecordAsync(string id)
        {
            return database.Table<ListItems>().Where(i => i.Id == id).FirstOrDefault();
        }

    
        public void deleteJobNo()
        {
            database.Query<JobNoList>("DELETE FROM [JobNoList]");
        }

      
        public void deleteMainMenu()
        {
            database.Query<ListItems>("DELETE FROM ListItems");
        }

        public void deleteRecords(string category)
        {
            database.Query<ListItems>("DELETE FROM ListItems WHERE [Category] = ?", category);
        }

        public void deleteRecordSummary(string type)
        {
            database.Query<SummaryItems>("DELETE FROM SummaryItems WHERE [type] = ?" , type);
        }
        
        public void deleteSummaryFleet(string id)
        {
            database.Query<SummaryItems>("DELETE FROM SummaryItems WHERE [id] = ?", id);
        }

        public void deleteDetail(string id)
        {
            database.Query<DetailItems>("DELETE FROM DetailItems WHERE [id] = ?",id);
        }

        public void deleteHaulageSummary(string type)
        {
            database.Query<SummaryItems>("DELETE FROM SummaryItems WHERE [Type] = ?", type);
        }

        public void deleteRecordDetails()
        {
            database.Query<DetailItems>("DELETE FROM DetailItems");
        }

      
        public int SaveMenuAsync(ListItems item)
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

        public int DeleteSummaryItem(SummaryItems item){
            return database.Delete(item);
        }

        public int DeleteDetailItem(DetailItems item)
        {
            return database.Delete(item);
        }
        
    }
}
