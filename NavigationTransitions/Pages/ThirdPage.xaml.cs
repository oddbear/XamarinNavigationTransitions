using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace NavigationTransitions
{
    public partial class ThirdPage : ContentPage
    {
        void Handle_Back_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }

        void Handle_Root_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PopToRootAsync();
        }

        void Handle_Navigate_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new ThirdPage());
        }

        public ThirdPage()
        {
            InitializeComponent();
            this.BindingContext = new ThirdPageModel();

            var random = new Random();
            BackgroundColor = Color.FromRgb(random.Next(0xFF), random.Next(0xFF), random.Next(0xFF));
        }
    }
}
