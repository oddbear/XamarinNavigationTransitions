using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace NavigationTransitions
{
    public class SecondPageModel
    {
        public ICommand NavigateToThirdPage { get; private set; }
        public ICommand NavigateBack { get; private set; }

        public SecondPageModel()
        {
            var mainPage = Application.Current.MainPage;
            var sharedLock = new SharedLock();

            NavigateToThirdPage = new SingleLockCommand(() => mainPage.Navigation.PushAsync(new ThirdPage()), sharedLock);
            NavigateBack = new SingleLockCommand(() => Task.FromResult(mainPage.SendBackButtonPressed()), sharedLock);
        }
    }
}
