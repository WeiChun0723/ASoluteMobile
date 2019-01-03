using System;
using System.Collections.Generic;
using Syncfusion.XForms.DataForm;
using Xamarin.Forms;

namespace ASolute_Mobile.testing
{
    public partial class dataform : ContentPage
    {
        public dataform()
        {
            InitializeComponent();

            BindingContext = new ContactsInfoViewModel();
            dataForm.AutoGeneratingDataFormItem += DataForm_AutoGeneratingDataFormItem;

        }




        void DataForm_AutoGeneratingDataFormItem(object sender, Syncfusion.XForms.DataForm.AutoGeneratingDataFormItemEventArgs e)
        {
            if (e.DataFormItem != null && e.DataFormItem.Name == "FirstName")
            {
                var list = new List<string>();
                list.Add("Home");
                list.Add("Food");
                list.Add("Utilities");
                list.Add("Education");
                (e.DataFormItem as DataFormDropDownItem).ItemsSource = list;


            }
        }
    }
}
