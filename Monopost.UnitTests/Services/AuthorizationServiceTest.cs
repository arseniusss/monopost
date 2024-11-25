using Monopost.BLL.Services.Implementations;
using Monopost.DAL.Repositories.Interfaces;
using Moq;
using Monopost.DAL.Entities;

namespace Monopost.UnitTests.Services
{
    public class AuthorizationServiceTest
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly AuthorizationService _authService;

        public AuthorizationServiceTest()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _authService = new AuthorizationService(_mockUserRepository.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsSuccess()
        {
            var email = "test@example.com";
            var password = "ValidPassword123!";
            var firstName = "Name";
            var lastName = "LastName";
            int age = 19;
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Email = email,
                Password = hashedPassword,
                FirstName = firstName,
                LastName = lastName,
                Age = age
            };

            _mockUserRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User> { user });

            var result = await _authService.Login(email, password);

            Assert.True(result.success);
            Assert.Equal("Login successful", result.message);
        }

        [Fact]
        public async Task Login_InvalidEmail_ReturnsFailure()
        {
            _mockUserRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User>());

            var result = await _authService.Login("nonExistentTestEmail@example.com", "anypassword");

            Assert.False(result.success);
            Assert.Equal("No user found with this email.", result.message);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsFailure()
        {
            var email = "testEmail@example.com";
            var correctPassword = "ValidPassword123!";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
            var firstName = "Name";
            var lastName = "LastName";
            int age = 19;
            var user = new User
            {
                Email = email,
                Password = hashedPassword,
                FirstName = firstName,
                LastName = lastName,
                Age = age
            };

            _mockUserRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User> { user });

            var result = await _authService.Login(email, "WrongPassword456!");

            Assert.False(result.success);
            Assert.Equal("Incorrect password.", result.message);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            _mockUserRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User>());

            var user = new User
            {
                Email = "ruban.kateryna@example.com",
                Password = "ValidPassword123!",
                FirstName = "Kateryna",
                LastName = "Ruban",
                Age = 19
            };

            var result = await _authService.Register(user, user.Password);

            Assert.True(result.success);
            Assert.Equal("Registration successful!", result.message);
            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Register_ExistingEmail_ReturnsFailure()
        {
            var existingUser = new User
            {
                Email = "ruban.kateryna@example.com",
                Password = "ValidPassword123!",
                FirstName = "Kateryna",
                LastName = "Ruban",
                Age = 19
            };


            _mockUserRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User> { existingUser });

            var newUser = new User
            {
                Email = "ruban.kateryna@example.com",
                Password = "NewPassword456!",
                FirstName = "Kate",
                LastName = "Ruban",
                Age = 25
            };

            var result = await _authService.Register(newUser, newUser.Password);

            Assert.False(result.success);
            Assert.Equal("User with this email already exists.", result.message);
        }


        [Theory]
        [InlineData("", "Password123!", false)]
        [InlineData("invalidemail", "Password123!", false)]
        [InlineData("test@example.com", "", false)]
        [InlineData("test@example.com", "short", false)]
        [InlineData("test@example.com", "NoDigits!", false)]
        [InlineData("test@example.com", "nouppercaseletter123!", false)]
        [InlineData("test@example.com", "NOLOWERCASELETTER123!", false)]
        [InlineData("test@example.com", "NoSpecialCharacter123", false)]
        public void ValidateUserData_InvalidCredentials_ReturnsFalse(string email, string password, bool expectedValidity)
        {
            var user = new User
            {
                Email = email,
                Password = password,
                FirstName = "Kateryna",
                LastName = "Ruban",
                Age = 25
            };

            var (isValid, _) = _authService.ValidateUserData(user, password);

            Assert.Equal(expectedValidity, isValid);
        }
    }
}
