using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace NavigationTransitions
{
    public partial class SecondPage : ContentPage
    {
        void Handle_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }

        public SecondPage()
        {
            InitializeComponent();
        }
    }
}

