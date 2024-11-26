using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using System.Windows.Controls;
using Serilog;
using System.Windows;
using Monopost.DAL.Entities;
using Monopost.PresentationLayer.Helpers;
using System.Text.RegularExpressions;
using System.Linq;

namespace Monopost.Web.Views
{
    public partial class RegisterPage : Page
    {
        private MainWindow _mainWindow;
        private RegisterPage _registerPage;
        private readonly IUserRepository _userRepository;
        public static ILogger logger = LoggerConfig.GetLogger();

        public RegisterPage(MainWindow mainWindow, IUserRepository userRepository)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _userRepository = userRepository;

            SetInitialFieldTexts();
        }

        private void SetInitialFieldTexts()
        {
            NameTextBox.Text = "Name";
            LastNameTextBox.Text = "Last name";
            EmailTextBox.Text = "Email";
            AgeTextBox.Text = "Age";
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && (textBox.Text == "Name" || textBox.Text == "Last name" || textBox.Text == "Email" || textBox.Text == "Age"))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }


        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                switch (textBox.Name)
                {
                    case "NameTextBox":
                        textBox.Text = "Name";
                        break;
                    case "LastNameTextBox":
                        textBox.Text = "Last name";
                        break;
                    case "EmailTextBox":
                        textBox.Text = "Email";
                        break;
                    case "AgeTextBox":
                        textBox.Text = "Age";
                        break;
                }
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.Clear();
                passwordBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null && string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                passwordBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
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
                MessageBox.Show("Incorrect input format. All fields should be non-empty, age should be an integer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                PasswordTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                PasswordConfirmTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            var (isValid, errorMessage) = ValidateUser(newUser);
            if (!isValid)
            {
                MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existingUser = await _userRepository.GetAllAsync();
            var user = existingUser.FirstOrDefault(u => u.Email == newUser.Email);
            if (user != null)
            {
                MessageBox.Show("User with this email already exists.", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

            await _userRepository.AddAsync(newUser);

            logger.Information($"User {newUser.Email} registered successfully.");


            MessageBox.Show("Registration Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            _mainWindow.NavigateToLogInPage();
        }

        private (bool IsValid, string ErrorMessage) ValidateUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return (false, "Email is required.");
            }
            if (!Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return (false, "Invalid email format.");
            }

            if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 8)
            {
                return (false, "Password must be at least 8 characters long.");
            }
            if (!Regex.IsMatch(user.Password, @"[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter.");
            }
            if (!Regex.IsMatch(user.Password, @"[a-z]"))
            {
                return (false, "Password must contain at least one lowercase letter.");
            }
            if (!Regex.IsMatch(user.Password, @"[0-9]"))
            {
                return (false, "Password must contain at least one digit.");
            }
            if (!Regex.IsMatch(user.Password, @"[\W_]"))
            {
                return (false, "Password must contain at least one special character (e.g., @, #, $, etc.).");
            }

            if (user.Age < 18 || user.Age > 120)
            {
                return (false, "Age must be between 18 and 120.");
            }

            if (string.IsNullOrWhiteSpace(user.FirstName) || user.FirstName.Length > 50)
            {
                return (false, "First name can't be empty or longer than 50 characters.");
            }

            if (string.IsNullOrWhiteSpace(user.LastName) || user.LastName.Length > 50)
            {
                return (false, "Last name can't be empty or longer than 50 characters.");
            }

            return (true, string.Empty);
        }
    }
}
