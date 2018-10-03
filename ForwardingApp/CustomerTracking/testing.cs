using System;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public class testing : ViewCell
    {
        public testing()
        {
        }

        protected override void OnBindingContextChanged()
        {

            StackLayout cellWrapper = new StackLayout()
            {
                //Padding = new Thickness(10, 10, 10, 10),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            bool firstSummaryLine = true;

            if (Ultis.Settings.ListType == "provider_List")
            {
                foreach (ProviderInfo items in providers)
                {
                    Label label = new Label();
                    label.FontAttributes = FontAttributes.Bold;
                    label.Text = items.Name;
                    cellWrapper.Children.Add(label);
                }

            }
            else
            {

                foreach (SummaryItems items in summaryRecord)
                {
                    Label label = new Label();

                    if (items.Caption == "" || items.Caption == "Job No." || items.Caption == "Consignee" || Ultis.Settings.ListType.Equals("category_List"))
                    {
                        label.Text = items.Value;

                    }
                    else if (items.Caption == "Action" || items.Display == false)
                    {
                        label.IsVisible = false;
                    }
                    else
                    {
                        label.Text = items.Caption + ": " + items.Value;
                    }

                    if (firstSummaryLine)
                    {

                        firstSummaryLine = false;
                        label.FontAttributes = FontAttributes.Bold;
                    }


                    cellWrapper.Children.Add(label);

                    if (!(String.IsNullOrEmpty(items.BackColor)))
                    {
                        cellWrapper.BackgroundColor = Color.FromHex(items.BackColor);
                        color = items.BackColor;
                    }

                }
            }

            absoluteLayout.Children.Add(cellWrapper);

            MenuItem menuItem = new MenuItem()
            {
                Text = "Delete",
                IsDestructive = true
            };

            menuItem.Clicked += (sender, e) =>
            {

                AppMenu menu = (AppMenu)((MenuItem)sender).BindingContext;
                int results = 0;
                results = App.Database.DeleteMenu(menu);
                if (results > 0)
                {
                    ListView parent = (ListView)this.Parent;
                    ObservableCollection<AppMenu> itemSource = ((ObservableCollection<AppMenu>)parent.ItemsSource);
                    if (itemSource.Contains(menu))
                    {
                        itemSource.Remove(menu);
                    }

                    callWebService(menu.menuId);
                }
            };

            menuItem.BindingContextChanged += (sender, e) => {
                if (((MenuItem)sender).BindingContext != null)
                {

                }
            };

            this.ContextActions.Add(menuItem);



            View = new Frame
            {
                Content = absoluteLayout,
                HasShadow = true,
                Margin = 5,

            };

            if (!(String.IsNullOrEmpty(color)))
            {
                View.BackgroundColor = Color.FromHex(color);
            }

        }
    }
    }
}
