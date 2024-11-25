using Monopost.DAL.Repositories.Interfaces;
using Monopost.DAL.Entities;
using Monopost.Logging;
using System.Text.RegularExpressions;
using Serilog;
using Monopost.BLL.Helpers;
using Monopost.BLL.Services.Interfaces;

namespace Monopost.BLL.Services.Implementations
{
    public class AuthorizationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public AuthorizationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _logger = LoggerConfig.GetLogger();
        }

        public async Task<(bool success, string message)> Login(string email, string password)
        {
             var existingUsers = await _userRepository.GetAllAsync();
             var user = existingUsers.FirstOrDefault(u => u.Email == email);

             if (user == null)
             {
                 _logger.Warning($"Login attempt failed: No user found with email {email}");
                 return (false, "No user found with this email.");
             }

             bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, user.Password);
             if (!passwordMatch)
             {
                 _logger.Warning($"Login attempt failed: Incorrect password for email {email}");
                 return (false, "Incorrect password.");
             }

             await UserSession.SetUserId(_userRepository, email);
             _logger.Information($"User successfully logged in: {email}");
             return (true, "Login successful");
           
        }

        public async Task LoginAsGuestAsync()
        {
            await UserSession.SetUserId(_userRepository, null);
            _logger.Information("Guest user logged in");
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var existingUsers = await _userRepository.GetAllAsync();
            var user = existingUsers.FirstOrDefault(u => u.Email == email);

            if (user == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public async Task<(bool success, string message)> Register(User user, string confirmPassword)
        {
             var (isValid, errorMessage) = ValidateUserData(user, confirmPassword);
             if (!isValid)
             {
                 return (false, errorMessage);
             }

             var existingUsers = await _userRepository.GetAllAsync();
             if (existingUsers.Any(u => u.Email == user.Email))
             {
                 return (false, "User with this email already exists.");
             }

             user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
             await _userRepository.AddAsync(user);

             _logger.Information($"User {user.Email} registered successfully.");
             return (true, "Registration successful!");
           
        }

        public (bool isValid, string errorMessage) ValidateUserData(User user, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return (false, "Email is required.");
            }
            if (!Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return (false, "Invalid email format.");
            }

            if (user.Password != confirmPassword)
            {
                return (false, "Passwords do not match.");
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