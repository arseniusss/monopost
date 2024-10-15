using Microsoft.EntityFrameworkCore;
using Monopost.BLL.Models;
using Monopost.BLL.Services;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Implementations;
using Monopost.DAL.DataAccess;
using Monopost.Logging;
using Serilog;

namespace Monopost.UnitTests.Services
{
    public class TemplateManagementServiceTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly TemplateManagementService _templateManagementService;
        private readonly UserRepository _userRepository;

        public TemplateManagementServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.EnsureCreated();

            LoggerConfig.ConfigureLogging();

            var templateRepository = new TemplateRepository(_dbContext);
            _userRepository = new UserRepository(_dbContext);
            var templateFileRepository = new TemplateFileRepository(_dbContext);

            _templateManagementService = new TemplateManagementService(templateRepository, templateFileRepository, _userRepository);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            Log.CloseAndFlush();
        }

        [Fact]
        public async Task AddTemplateAsync_ShouldAddTemplate_WhenValidModel()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var templateModel = new TemplateModel
            {
                Id = 1,
                Name = "Template 1",
                Text = "This is a test template.",
                AuthorId = user.Id
            };

            var result = await _templateManagementService.AddTemplateAsync(templateModel);

            Assert.True(result.Success);
            Assert.Equal("Template created successfully.", result.Message);

            var templates = await _dbContext.Templates.ToListAsync();
            Assert.Single(templates);
            Assert.Equal(templateModel.Name, templates.First().Name);
        }

        [Fact]
        public async Task AddTemplateAsync_ShouldFail_WhenUserDoesNotExist()
        {
            var templateModel = new TemplateModel
            {
                Id = 2,
                Name = "Template 2",
                Text = "Another template.",
                AuthorId = 999
            };

            var result = await _templateManagementService.AddTemplateAsync(templateModel);

            Assert.False(result.Success);
            Assert.Equal("Invalid AuthorId: User does not exist.", result.Message);
        }

        [Fact]
        public async Task UpdateTemplateAsync_ShouldUpdateTemplate_WhenValidModel()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var templateModel = new TemplateModel
            {
                Id = 1,
                Name = "Template 1",
                Text = "This is a test template.",
                AuthorId = user.Id
            };

            await _templateManagementService.AddTemplateAsync(templateModel);

            templateModel.Text = "Updated template text.";

            var result = await _templateManagementService.UpdateTemplateAsync(templateModel);

            Assert.True(result.Success);
            Assert.Equal("Template updated successfully.", result.Message);

            var updatedTemplate = await _dbContext.Templates.FirstOrDefaultAsync(t => t.Id == templateModel.Id);
            Assert.Equal("Updated template text.", updatedTemplate?.Text);
        }

        [Fact]
        public async Task DeleteTemplateAsync_ShouldDeleteTemplate_WhenTemplateExists()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var templateModel = new TemplateModel
            {
                Id = 1,
                Name = "Template 1",
                Text = "This is a test template to be deleted.",
                AuthorId = user.Id
            };

            await _templateManagementService.AddTemplateAsync(templateModel);

            var result = await _templateManagementService.DeleteTemplateAsync(templateModel.Id);

            Assert.True(result.Success);
            Assert.Equal("Template and its files deleted successfully.", result.Message);

            var templates = await _dbContext.Templates.ToListAsync();
            Assert.Empty(templates);
        }

        [Fact]
        public async Task DeleteTemplateAsync_ShouldFail_WhenTemplateDoesNotExist()
        {
            var result = await _templateManagementService.DeleteTemplateAsync(999);
            Assert.False(result.Success);
            Assert.Equal("Template not found.", result.Message);
        }

        [Fact]
        public async Task GetAllTemplatesAsync_ShouldReturnAllTemplates_WhenTemplatesExist()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var template1 = new TemplateModel { Id = 1, Name = "Template 1", Text = "Text 1", AuthorId = user.Id };
            var template2 = new TemplateModel { Id = 2, Name = "Template 2", Text = "Text 2", AuthorId = user.Id };

            await _templateManagementService.AddTemplateAsync(template1);
            await _templateManagementService.AddTemplateAsync(template2);

            var result = await _templateManagementService.GetAllTemplatesAsync();

            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            Assert.Equal("Templates retrieved successfully.", result.Message);
        }

        [Fact]
        public async Task GetTemplatesByUserIdAsync_ShouldReturnUserTemplates_WhenTemplatesExist()
        {
            var user1 = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            var user2 = new User { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", Password = "password", Age = 28 };
            await _dbContext.Users.AddRangeAsync(user1, user2);
            await _dbContext.SaveChangesAsync();

            var template1 = new TemplateModel { Id = 1, Name = "Template 1", Text = "Text 1", AuthorId = user1.Id };
            var template2 = new TemplateModel { Id = 2, Name = "Template 2", Text = "Text 2", AuthorId = user1.Id };
            var template3 = new TemplateModel { Id = 3, Name = "Template 3", Text = "Text 3", AuthorId = user2.Id };

            await _templateManagementService.AddTemplateAsync(template1);
            await _templateManagementService.AddTemplateAsync(template2);
            await _templateManagementService.AddTemplateAsync(template3);

            var result = await _templateManagementService.GetTemplatesByUserIdAsync(user1.Id);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            Assert.Equal("Templates retrieved successfully.", result.Message);

            if (result.Data == null)
            {
                Assert.Fail("result.Data should not be null.");
            }
            else
            {
                Assert.All(result.Data, template => Assert.Equal(user1.Id, template.AuthorId));
            }
        }

        [Fact]
        public async Task GetTemplatesByUserIdAsync_ShouldReturnNoTemplates_WhenNoTemplatesExist()
        {
            var result = await _templateManagementService.GetTemplatesByUserIdAsync(999);

            Assert.False(result.Success);
            Assert.Equal("No templates found for the specified user.", result.Message);
        }

        [Fact]
        public async Task GetTemplateFilesByTemplateIdAsync_ShouldReturnTemplateFiles_WhenTemplateFilesExist()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var templateModel = new TemplateModel
            {
                Id = 1,
                Name = "Template 1",
                Text = "Template with files.",
                AuthorId = user.Id,
                TemplateFiles = new List<TemplateFileModel>
                {
                    new TemplateFileModel { FileName = "File1.txt", FileData = new byte[] { 0x01, 0x02 } },
                    new TemplateFileModel { FileName = "File2.txt", FileData = new byte[] { 0x03, 0x04 } }
                }
            };

            await _templateManagementService.AddTemplateAsync(templateModel);

            var result = await _templateManagementService.GetTemplateFilesByTemplateIdAsync(templateModel.Id);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            Assert.Equal("Template files retrieved successfully.", result.Message);
        }

        [Fact]
        public async Task GetTemplateFilesByTemplateIdAsync_ShouldReturnNoFiles_WhenNoTemplateFilesExist()
        {
            var result = await _templateManagementService.GetTemplateFilesByTemplateIdAsync(999);

            Assert.False(result.Success);
            Assert.Equal("Template not found.", result.Message);
        }

        [Fact]
        public async Task AddTemplateAsync_ShouldFail_WhenTemplateWithSameIdExists()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var templateModel1 = new TemplateModel
            {
                Id = 1,
                Name = "Template 1",
                Text = "This is a test template.",
                AuthorId = user.Id
            };

            var templateModel2 = new TemplateModel
            {
                Id = 1,
                Name = "Template 2",
                Text = "This template has the same Id.",
                AuthorId = user.Id
            };

            await _templateManagementService.AddTemplateAsync(templateModel1);

            var result = await _templateManagementService.AddTemplateAsync(templateModel2);

            Assert.False(result.Success);
            Assert.Equal("Template with id 1 already exists.", result.Message);
        }

        [Fact]
        public async Task AddTemplateAsync_ShouldFail_WhenNameOrTextIsNull()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var templateModel = new TemplateModel
            {
                Id = 1,
                Name = "",
                Text = "Valid text.",
                AuthorId = user.Id
            };

            var result = await _templateManagementService.AddTemplateAsync(templateModel);

            Assert.False(result.Success);
            Assert.Equal("Name and Text are required.", result.Message);

            templateModel.Name = "Valid name";
            templateModel.Text = "";

            result = await _templateManagementService.AddTemplateAsync(templateModel);

            Assert.False(result.Success);
            Assert.Equal("Name and Text are required.", result.Message);
        }

        [Fact]
        public async Task UpdateTemplateAsync_ShouldFail_WhenTemplateNotFound()
        {
            var templateModel = new TemplateModel
            {
                Id = 999,
                Name = "Non-existent template",
                Text = "Trying to update a non-existent template.",
                AuthorId = 1
            };

            var result = await _templateManagementService.UpdateTemplateAsync(templateModel);

            Assert.False(result.Success);
            Assert.Equal("Template not found.", result.Message);
        }

        [Fact]
        public async Task GetTemplateFilesByTemplateIdAsync_ShouldFail_WhenTemplateNotFound()
        {
            var result = await _templateManagementService.GetTemplateFilesByTemplateIdAsync(999);

            Assert.False(result.Success);
            Assert.Equal("Template not found.", result.Message);
        }

        [Fact]
        public async Task GetAllTemplatesAsync_ShouldReturnEmptyList_WhenNoTemplatesExist()
        {
            var result = await _templateManagementService.GetAllTemplatesAsync();

            Assert.True(result.Success);
            Assert.Equal(result.Data?.Count(), 0);
            Assert.Equal("Templates retrieved successfully.", result.Message);
        }

        [Fact]
        public async Task GetTemplatesByUserIdAsync_ShouldReturnFailure_WhenUserHasNoTemplates()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "password", Age = 30 };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var result = await _templateManagementService.GetTemplatesByUserIdAsync(user.Id);

            Assert.False(result.Success);
            Assert.Equal("No templates found for the specified user.", result.Message);
        }

        [Fact]
        public async Task GetTemplatesByUserIdAsync_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            var result = await _templateManagementService.GetTemplatesByUserIdAsync(999);

            Assert.False(result.Success);
            Assert.Equal("No templates found for the specified user.", result.Message);
        }
    }
}