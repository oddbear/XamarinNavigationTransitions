using System;
using NavigationTransitions.Droid;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomTransitionsRenderer))]
namespace NavigationTransitions.Droid
{
    public class CustomTransitionsRenderer : Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer
    {
        protected override void SetupPageTransition(Android.Support.V4.App.FragmentTransaction transaction, bool isPush)
        {
            if (isPush)
                transaction.SetCustomAnimations(Resource.Animator.move_in_left, 0);
            else //prevView enter:
                transaction.SetCustomAnimations(Resource.Animator.move_in_left, 0);
        }
    }
}
