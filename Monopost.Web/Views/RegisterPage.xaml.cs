using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using System.Windows.Controls;
using Serilog;
using System.Windows;
using Monopost.DAL.Entities;
using Monopost.PresentationLayer.Helpers;

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
        }

        private async void SignUp_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordTextBox.Password.Trim();
            string firstName = NameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string ageText = AgeTextBox.Text.Trim();
           

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(ageText) || !int.TryParse(ageText, out int age))
            {
                MessageBox.Show("Please provide valid input.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await UserSession.SetUserId(_userRepository, email);

            int userId = UserSession.GetUserId();
            var newUser = new User
            {
                Id = userId,
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
            //await SendRegistrationConfirmationEmailAsync(newUser.Email);
            //MessageBox.Show("Registration Successful! A confirmation email has been sent.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            
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

            if (!user.Email.Contains("@"))
            {
                return (false, "Invalid email format.");
            }

            if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 6)
            {
                return (false, "Password must be at least 6 characters long.");
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




        //private async Task SendRegistrationConfirmationEmailAsync(string email)
        //{
        //    var emailSubject = "Registration Successful!";
        //    var emailBody = "Thank you for registering. Your account has been successfully created.";

        //    using (var client = new System.Net.Mail.SmtpClient("smtp.gmail.com"))
        //    {
        //        client.Port = 587;
        //        client.Credentials = new System.Net.NetworkCredential("your-email@gmail.com", "your-password");

        //        client.EnableSsl = true;

        //        var mailMessage = new System.Net.Mail.MailMessage
        //        {
        //            From = new System.Net.Mail.MailAddress("your-email@gmail.com"),
        //            Subject = emailSubject,
        //            Body = emailBody,
        //            IsBodyHtml = false 
        //        };

        //        mailMessage.To.Add(email);

        //        try
        //        {
        //            await client.SendMailAsync(mailMessage);
        //            Console.WriteLine("Confirmation email sent successfully!");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"An error occurred while sending the email: {ex.Message}");
        //        }
        //    }
        //}
    }
}
