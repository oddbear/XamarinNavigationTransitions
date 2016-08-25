using System;
using System.Windows.Input;

namespace NavigationTransitions
{
    public class MainPageModel
    {
        public ICommand NavigateToSecondPage { get; private set; }

        public MainPageModel()
        {
            NavigateToSecondPage = new SingleLockCommand(() => App.Current.MainPage.Navigation.PushAsync(new SecondPage()));
        }
    }
}
