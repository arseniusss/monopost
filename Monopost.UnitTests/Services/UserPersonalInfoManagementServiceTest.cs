using Microsoft.EntityFrameworkCore;
using Monopost.BLL.Models;
using Monopost.BLL.Services.Implementations;
using Monopost.DAL.DataAccess;
using Monopost.DAL.Repositories.Implementations;
using Monopost.DAL.Entities;


namespace Monopost.UnitTests.Services
{
    public class UserPersonalInfoManagementServiceTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly UserPersonalInfoManagementService _userPersonalInfoManagementService;
        private readonly UserRepository _userRepository;

        public UserPersonalInfoManagementServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureCreated();

            _userRepository = new UserRepository(_dbContext);
            _userPersonalInfoManagementService = new UserPersonalInfoManagementService(_userRepository);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task UpdateUserPersonalInfoAsync_ValidUpdate_ReturnsSuccess()
        {
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "password",
                Age = 30
            };

            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var updateModel = new UserPersonalInfoModel
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Doe",
                Age = 35
            };

            var result = await _userPersonalInfoManagementService.UpdateUserPersonalInfoAsync(updateModel);

            Assert.True(result.Success);
            Assert.Equal("User updated successfully.", result.Message);

            var updatedUser = await _userRepository.GetByIdAsync(user.Id);
            Assert.Equal("Jane", updatedUser.FirstName);
            Assert.Equal("Doe", updatedUser.LastName);
            Assert.Equal(35, updatedUser.Age);
        }

        [Fact]
        public async Task UpdateUserPersonalInfoAsync_UserNotFound_ReturnsFailure()
        {
            var updateModel = new UserPersonalInfoModel
            {
                Id = 1000,
                FirstName = "Jane",
                LastName = "Doe",
                Age = 35
            };

            var result = await _userPersonalInfoManagementService.UpdateUserPersonalInfoAsync(updateModel);

            Assert.False(result.Success);
            Assert.Equal("User with such Id does not exist.", result.Message);
        }

        [Fact]
        public async Task UpdateUserPersonalInfoAsync_InvalidFirstName_ReturnsFailure()
        {
            var updateModel = new UserPersonalInfoModel
            {
                Id = 1,
                FirstName = "",
                LastName = "Doe",
                Age = 35
            };

            var result = await _userPersonalInfoManagementService.UpdateUserPersonalInfoAsync(updateModel);

            Assert.False(result.Success);
            Assert.Equal("First Name is required.", result.Message);
        }

        [Fact]
        public async Task UpdateUserPersonalInfoAsync_InvalidAge_ReturnsFailure()
        {
            var updateModel = new UserPersonalInfoModel
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Doe",
                Age = 0
            };

            var result = await _userPersonalInfoManagementService.UpdateUserPersonalInfoAsync(updateModel);

            Assert.False(result.Success);
            Assert.Equal("Valid Age is required.", result.Message);
        }
    }
}
