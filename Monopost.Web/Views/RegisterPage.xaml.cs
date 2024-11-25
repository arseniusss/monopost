using Monopost.BLL.Services.Interfaces;
using Monopost.DAL.Entities;
using System.Windows;
using System.Windows.Controls;
using Monopost.BLL.Services.Implementations;
using Monopost.DAL.Repositories.Interfaces;

namespace Monopost.Web.Views
{
    public partial class RegisterPage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly IAuthenticationService _authService;


        public RegisterPage(MainWindow mainWindow, IUserRepository userRepository)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _authService = new AuthorizationService(userRepository);
        }

        private async void SignUp_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordTextBox.Password.Trim();
            string confirmPassword = PasswordConfirmTextBox.Password.Trim();
            string firstName = NameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string ageText = AgeTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(ageText) ||
                !int.TryParse(ageText, out int age))
            {
                MessageBox.Show("Incorrect input format. All fields should be non-empty, age should be an integer.",
                              "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newUser = new User
            {
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                Age = age
            };

            var (success, message) = await _authService.Register(newUser, confirmPassword);

            if (success)
            {
                MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _mainWindow.NavigateToLogInPage();
            }
            else
            {
                if (message.Contains("Passwords do not match"))
                {
                    PasswordTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                    PasswordConfirmTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                }
                MessageBox.Show(message, "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}