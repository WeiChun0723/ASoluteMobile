using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class ListViewScreen : ContentPage
    {
        public Label label;

        public ListViewScreen()
        {
            InitializeComponent();

            StackLayout mainLayout = new StackLayout();

            label = new Label();

            mainLayout.Children.Add(label);

            Content = new ScrollView
            {
                Content = mainLayout
            };
        }
    }
}
