using System;
using System.Threading.Tasks;
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
            var mainPage = Application.Current.MainPage;
            var sharedLock = new SharedLock();

            NavigateToThirdPage = new SingleLockCommand(() => mainPage.Navigation.PushAsync(new ThirdPage()), sharedLock);
            NavigateBack = new SingleLockCommand(() => Task.FromResult(mainPage.SendBackButtonPressed()), sharedLock);
            NavigateToRoot = new SingleLockCommand(mainPage.Navigation.PopToRootAsync, sharedLock);
        }
    }
}
