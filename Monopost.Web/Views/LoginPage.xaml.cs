using System.Windows;
using System.Windows.Controls;

namespace Monopost.Web.Views
{
    public partial class LoginPage : Page
    {
        private MainWindow _mainWindow;

        public LoginPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateToMainContent();
        }
    }
}
