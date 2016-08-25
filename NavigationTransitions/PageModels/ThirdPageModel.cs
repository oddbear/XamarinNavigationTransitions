using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace NavigationTransitions
{
    public class ThirdPageModel
    {
        public ICommand NavigateToThirdPage { get; private set; }
        public ICommand NavigateBack { get; private set; }
        public ICommand NavigateToRoot { get; private set; }

        public ThirdPageModel()
        {
            var sharedLock = new SharedLock();
            NavigateToThirdPage = new SingleLockCommand(() => App.Current.MainPage.Navigation.PushAsync(new ThirdPage()), sharedLock);
            NavigateBack = new Command(() => {
                if (sharedLock.TakeLock())
                {
                    try
                    {
                        App.Current.MainPage.SendBackButtonPressed();
                    }
                    finally
                    {
                        sharedLock.ReleaseLock();
                    }
                }
            });
            NavigateToRoot = new SingleLockCommand(() => App.Current.MainPage.Navigation.PopToRootAsync(), sharedLock);
        }
    }
}
