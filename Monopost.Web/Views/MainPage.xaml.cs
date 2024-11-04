using Monopost.Web.Commands;
using System.Windows;
using System.Windows.Controls;

namespace Monopost.Web.Views
{
    public partial class MainPage : Page
    {
        public RelayCommand NavigateProfileCommand { get; }
        public RelayCommand NavigateMonobankCommand { get; }
        public RelayCommand NavigatePostingCommand { get; }
        public RelayCommand NavigateAdminCommand { get; }

        public MainPage()
        {
            InitializeComponent();
            DataContext = this; 

            NavigateProfileCommand = new RelayCommand(_ => NavigateToProfilePage());
            NavigateMonobankCommand = new RelayCommand(_ => NavigateToMonobankPage());
            NavigatePostingCommand = new RelayCommand(_ => NavigateToPostingPage());
            NavigateAdminCommand = new RelayCommand(_ => NavigateToAdminPage());
        }

        private void NavigateToProfilePage()
        {
            MainFrame.Navigate(new ProfilePage());
        }

        private void NavigateToMonobankPage()
        {
            MainFrame.Navigate(new MonobankPage());
        }

        private void NavigateToPostingPage()
        {
            MainFrame.Navigate(new PostingPage());
        }

        private void NavigateToAdminPage()
        {
            MainFrame.Navigate(new AdminPage());
        }
    }
}
