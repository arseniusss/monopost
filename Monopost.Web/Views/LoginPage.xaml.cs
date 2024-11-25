using Monopost.BLL.Services.Interfaces;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.BLL.Services.Implementations;
using System.Windows;
using System.Windows.Controls;

namespace Monopost.Web.Views
{
    public partial class LoginPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly IAuthenticationService _authService;

        public LoginPage(MainWindow mainWindow, IUserRepository userRepository)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _authService = new AuthorizationService(userRepository);
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordTextBox.Password;

            var (success, message) = await _authService.Login(email, password);

            if (!success)
            {
                MessageBox.Show(message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _mainWindow.NavigateToMainContent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateToRegisterPage();
        }

        private async void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await _authService.LoginAsGuestAsync();
            _mainWindow.NavigateToMainContent();
        }
    }
}
