using System;
namespace ASolute_Mobile.testing
{
    public class ContactsInfoViewModel
    {
        private ContactsInfo contactsInfo;

        public ContactsInfo ContactsInfo
        {
            get { return this.contactsInfo; }
            set { this.contactsInfo = value; }
        }

        public ContactsInfoViewModel()
        {
            this.contactsInfo = new ContactsInfo();
        }
    }
}
