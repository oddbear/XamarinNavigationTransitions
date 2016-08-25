using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace NavigationTransitions
{
    public partial class MainPage : ContentPage
    {
        void Handle_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new SecondPage());
        }

        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = new MainPageModel();
        }
    }
}

