using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class ChatRoom : ContentPage
    {
        public ChatRoom()
        {
            InitializeComponent();

            Title = "Chat Room";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            MessagingCenter.Subscribe<App>((App)Application.Current, "ChatReceived", (sender) => 
            {
                try
                {
                    LoadChat();
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "ChatReceived");
        }

        public void LoadChat()
        {
            /*ObservableCollection<ChatRecord> Item = new ObservableCollection<ChatRecord>(App.Database.Chats());

            chatList.ItemsSource = Item;
            chatList.HasUnevenRows = true;
            chatList.ScrollTo(Item[Item.Count - 1],ScrollToPosition.End,false);*/

        }

        public async void SendMessage(Object sender , EventArgs e)
        {
            if(!String.IsNullOrEmpty(chatEntry.Text))
            {
                List<string> id = new List<string>
                {
                    "976bab2a-d650-4139-bf9a-417f92c1633c"
                };

                Headings heading = new Headings
                {
                    en = "Testing"
                };

                Contents content = new Contents
                {
                    en = chatEntry.Text
                };

                ChatRoomObject chat = new ChatRoomObject
                {
                    include_player_ids = id,
                    app_id = "804c5448-99ec-4e95-829f-c98c0ea6acd9",
                    headings = heading,
                    contents =content
                };

                var webservice_content = await CommonFunction.PostRequestAsync(chat, "https://onesignal.com/api/v1/notifications", "");

                var testing = JObject.Parse(webservice_content);
                int result = Convert.ToInt32(testing["recipients"].ToString());

                /*ChatRecord sentChat = new ChatRecord
                {
                    Content = chatEntry.Text,
                    Sender = "Self",
                    updatedDate = DateTime.Now,
                    BackgroundColor = "#FFB6C1"
                };

                App.Database.SaveChat(sentChat);*/

                LoadChat();

            }
          
        }
    }
}
