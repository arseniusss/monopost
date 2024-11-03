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
            DataContext = this; // Set the data context for command binding

            NavigateProfileCommand = new RelayCommand(_ => NavigateToProfilePage());
            NavigateMonobankCommand = new RelayCommand(_ => NavigateToMonobankPage());
            NavigatePostingCommand = new RelayCommand(_ => NavigateToPostingPage());
            NavigateAdminCommand = new RelayCommand(_ => NavigateToAdminPage());
        }

        private void NavigateToProfilePage()
        {
            // Replace with your actual ProfilePage instance
            MainFrame.Navigate(new ProfilePage());
        }

        private void NavigateToMonobankPage()
        {
            // Replace with your actual MonobankPage instance
            MainFrame.Navigate(new MonobankPage());
        }

        private void NavigateToPostingPage()
        {
            // Replace with your actual PostingPage instance
            MainFrame.Navigate(new PostingPage());
        }

        private void NavigateToAdminPage()
        {
            // Replace with your actual AdminPage instance
            MainFrame.Navigate(new AdminPage());
        }
    }
}
