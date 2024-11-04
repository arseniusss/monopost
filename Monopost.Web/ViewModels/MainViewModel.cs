using System.Windows.Controls;
using System.Windows.Input;
using Monopost.Web.Commands;
using Monopost.Web.Views;

namespace Monopost.Web.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public Frame MainFrame { get; private set; }

        public ICommand NavigateProfileCommand { get; }
        public ICommand NavigateMonobankCommand { get; }
        public ICommand NavigatePostingCommand { get; }
        public ICommand NavigateAdminCommand { get; }

        public MainViewModel(Frame mainFrame)
        {
            MainFrame = mainFrame;

            NavigateProfileCommand = new RelayCommand(_ => MainFrame.Navigate(new ProfilePage()));
            NavigateMonobankCommand = new RelayCommand(_ => MainFrame.Navigate(new MonobankPage()));
            NavigatePostingCommand = new RelayCommand(_ => MainFrame.Navigate(new PostingPage()));
            NavigateAdminCommand = new RelayCommand(_ => MainFrame.Navigate(new AdminPage()));
        }
    }
}
