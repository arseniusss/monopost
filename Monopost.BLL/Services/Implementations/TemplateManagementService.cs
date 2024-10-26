using Monopost.BLL.Models;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using Serilog;

namespace Monopost.BLL.Services
{
    public class TemplateManagementService
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly IUserRepository _userRepository;
        public static ILogger logger = LoggerConfig.GetLogger();

        public TemplateManagementService(ITemplateRepository templateRepository, ITemplateFileRepository templateFileRepository, IUserRepository userRepository)
        {
            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<TemplateModel>> AddTemplateAsync(TemplateModel model)
        {
            logger.Information($"Trying to add a new template. AuthorId: {model.AuthorId}, Name: {model.Name}");
            var userExists = await _userRepository.UserExistsAsync(model.AuthorId);
            if (!userExists)
            {
                logger.Warning($"Result: Failure\nReason: Invalid AuthorId: User does not exist.");
                return new Result<TemplateModel>(false, "Invalid AuthorId: User does not exist.");
            }

            var templateExists = await _templateRepository.GetByIdAsync(model.Id);
            if (templateExists != null)
            {
                logger.Warning($"Result: Failure\nReason: Template with id {model.Id} already exists.");
                return new Result<TemplateModel>(false, $"Template with id {model.Id} already exists.");
            }

            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Text))
            {
                logger.Warning($"Result: Failure\nReason: Name and Text are required.");
                return new Result<TemplateModel>(false, "Name and Text are required.");
            }

            var template = new Template
            {
                Id = model.Id,
                Name = model.Name,
                Text = model.Text,
                AuthorId = model.AuthorId,
                TemplateFiles = model.TemplateFiles?.Select(tf => new TemplateFile
                {
                    FileName = tf.FileName,
                    FileData = tf.FileData
                }).ToList()
            };

            try
            {
                await _templateRepository.AddAsync(template);
                logger.Information($"Result: Success\nMessage: Template created successfully.");
                return new Result<TemplateModel>(true, "Template created successfully.", model);
            }
            catch (Exception e)
            {
                logger.Warning($"Result: Failure\nReason: {e.Message}");
                return new Result<TemplateModel>(false, e.Message);
            }
        }

        public async Task<Result> DeleteTemplateAsync(int templateId)
        {
            logger.Information($"Trying to delete template with id = {templateId}");
            var templateExists = await _templateRepository.GetByIdAsync(templateId);
            if (templateExists == null)
            {
                logger.Warning($"Result: Failure\nReason: Template not found.");
                return new Result(false, "Template not found.");
            }

            await _templateFileRepository.DeleteAllByTemplateIdAsync(templateId);
            await _templateRepository.DeleteAsync(templateId);

            logger.Information($"Result: Success\nMessage: Template and its files deleted successfully.");
            return new Result(true, "Template and its files deleted successfully.");
        }

        public async Task<Result> UpdateTemplateAsync(TemplateModel model)
        {
            logger.Information($"Trying to update template. Id: {model.Id}");
            if (model == null)
            {
                logger.Warning($"Result: Failure\nReason: Template model is required.");
                return new Result(false, "Template model is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Text))
            {
                logger.Warning($"Result: Failure\nReason: Name and Text are required.");
                return new Result(false, "Name and Text are required.");
            }

            var template = await _templateRepository.GetByIdAsync(model.Id);
            if (template == null)
            {
                logger.Warning($"Result: Failure\nReason: Template not found.");
                return new Result(false, "Template not found.");
            }
 
            template.Name = model.Name;
            template.Text = model.Text;

            await _templateRepository.UpdateAsync(template);
            logger.Information($"Result: Success\nMessage: Template updated successfully.");
            return new Result(true, "Template updated successfully.");
        }

        public async Task<Result<IEnumerable<TemplateModel>>> GetAllTemplatesAsync()
        {
            logger.Information($"Trying to retrieve all templates...");
            var templates = await _templateRepository.GetAllAsync();
            var templateDtos = templates.Select(t => new TemplateModel
            {
                Id = t.Id,
                Name = t.Name,
                Text = t.Text,
                AuthorId = t.AuthorId,
                TemplateFiles = t.TemplateFiles?.Select(tf => new TemplateFileModel
                {
                    Id = tf.Id,
                    FileName = tf.FileName,
                    FileData = tf.FileData,
                    TemplateId = tf.TemplateId
                }).ToList()
            }).ToList();

            logger.Information($"Result: Success\nMessage: Templates retrieved successfully.");
            return new Result<IEnumerable<TemplateModel>>(true, "Templates retrieved successfully.", templateDtos);
        }

        public async Task<Result<IEnumerable<TemplateFileModel>>> GetTemplateFilesByTemplateIdAsync(int templateId)
        {
            logger.Information($"Trying to get template files for templateId = {templateId}");

            var templateExists = await _templateRepository.GetByIdAsync(templateId);
            if(templateExists == null)
            {
                logger.Warning($"Result: Failure\nReason: Template not found.");
                return new Result<IEnumerable<TemplateFileModel>>(false, "Template not found.");
            }

            var templateFiles = await _templateFileRepository.GetTemplateFilesByTemplateIdAsync(templateId);
            var templateFileDtos = templateFiles.Select(tf => new TemplateFileModel
            {
                Id = tf.Id,
                FileName = tf.FileName,
                FileData = tf.FileData,
                TemplateId = tf.TemplateId
            }).ToList();

            logger.Information($"Result: Success\nMessage: Template files retrieved successfully.");
            return new Result<IEnumerable<TemplateFileModel>>(true, "Template files retrieved successfully.", templateFileDtos);
        }

        public async Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByUserIdAsync(int userId)
        {
            logger.Information($"Trying to get templates for userId = {userId}");
            var templates = await _templateRepository.GetTemplatesByAuthorIdAsync(userId);

            if (templates == null || !templates.Any())
            {
                logger.Warning($"Result: Failure\nReason: No templates found for the specified user.");
                return new Result<IEnumerable<TemplateModel>>(false, "No templates found for the specified user.");
            }

            var templateDtos = templates.Select(t => new TemplateModel
            {
                Id = t.Id,
                Name = t.Name,
                Text = t.Text,
                AuthorId = t.AuthorId,
                TemplateFiles = t.TemplateFiles?.Select(tf => new TemplateFileModel
                {
                    Id = tf.Id,
                    FileName = tf.FileName,
                    FileData = tf.FileData,
                    TemplateId = tf.TemplateId
                }).ToList()
            }).ToList();

            logger.Information($"Result: Success\nMessage: Templates retrieved successfully.");
            return new Result<IEnumerable<TemplateModel>>(true, "Templates retrieved successfully.", templateDtos);
        }
    }
}