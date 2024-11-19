using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using System.Windows;
using System.Windows.Controls;
using Serilog;
using Monopost.PresentationLayer.Helpers;

namespace Monopost.Web.Views
{
    public partial class LoginPage : Page
    {
        private MainWindow _mainWindow;
        private RegisterPage _registerPage;
        private readonly IUserRepository _userRepository;
        public static ILogger logger = LoggerConfig.GetLogger();

        public LoginPage(MainWindow mainWindow, IUserRepository userRepository)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _userRepository = userRepository;
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordTextBox.Password;

            var existingUser = await _userRepository.GetAllAsync();

            var user = existingUser.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                MessageBox.Show("No user found with this email.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!passwordMatch)
            {
                MessageBox.Show("Incorrect password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await UserSession.SetUserId(_userRepository, email);

            _mainWindow.NavigateToMainContent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateToRegisterPage();
        }

        private async void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await UserSession.SetUserId(_userRepository, null);
            _mainWindow.NavigateToMainContent();
        }
    }
}
