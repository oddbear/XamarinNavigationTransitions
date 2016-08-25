using System;
using NavigationTransitions.AppCompat;
using Xamarin.Forms;

//https://developer.android.com/reference/android/app/Activity.html#overridePendingTransition(int
//https://developer.android.com/reference/android/app/FragmentManager.html#beginTransaction()
//https://developer.android.com/reference/android/app/FragmentManager.html#executePendingTransactions()

//https://github.com/xamarin/Xamarin.Forms/blob/master/Xamarin.Forms.Platform.Android/AppCompat/NavigationPageRenderer.cs
[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomTransitionsRenderer))]
namespace NavigationTransitions.AppCompat
{
    public class CustomTransitionsRenderer : Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer
    {
        //SwitchContentAsync
        protected override void SetupPageTransition(Android.Support.V4.App.FragmentTransaction transaction, bool isPush)
        {
            if (isPush)
                transaction.SetCustomAnimations(Resource.Animator.move_in_left, 0);
            else //prevView enter:
                transaction.SetCustomAnimations(Resource.Animator.move_in_left, 0);
        }
    }
}
