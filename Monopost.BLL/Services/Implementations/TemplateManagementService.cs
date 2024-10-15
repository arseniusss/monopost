using Monopost.BLL.Models;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;

namespace Monopost.BLL.Services
{
    public class TemplateManagementService
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly IUserRepository _userRepository;

        public TemplateManagementService(ITemplateRepository templateRepository, ITemplateFileRepository templateFileRepository, IUserRepository userRepository)
        {
            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<TemplateModel>> AddTemplateAsync(TemplateModel model)
        {
            var userExists = await _userRepository.UserExistsAsync(model.AuthorId);
            if (!userExists)
            {
                return new Result<TemplateModel>(false, "Invalid AuthorId: User does not exist.");
            }
            try
            {
                await _templateFileRepository.GetByIdAsync(model.Id);
                return new Result<TemplateModel>(false, "Template with this Id already exists.");
            }
            catch { }

            if(model.Name == null || model.Name == string.Empty || model.Text == null || model.Text == string.Empty)
            {
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
                return new Result<TemplateModel>(true, "Template created successfully.", model);
            }
            catch (Exception e)
            {
                return new Result<TemplateModel>(false, e.Message);
            }
        }

        public async Task<Result> DeleteTemplateAsync(int templateId)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(templateId);
                await _templateFileRepository.DeleteAllByTemplateIdAsync(templateId);
                await _templateRepository.DeleteAsync(templateId);

                return new Result(true, "Template and its files deleted successfully.");
            }
            catch
            {
                return new Result(false, "Template not found.");
            }
        }

        public async Task<Result> UpdateTemplateAsync(TemplateModel model)
        {
            if (model == null)
            {
                return new Result(false, "Template model is required.");
            }

            if (model.Name == null || model.Name == string.Empty || model.Text == null || model.Text == string.Empty)
            {
                return new Result(false, "Name and Text are required.");
            }

            try
            {
                var template = await _templateRepository.GetByIdAsync(model.Id);
                template.Name = model.Name;
                template.Text = model.Text;

                await _templateRepository.UpdateAsync(template);
                return new Result(true, "Template updated successfully.");
            }
            catch
            {
                return new Result(false, "Template not found.");
            }            
        }

        public async Task<Result<IEnumerable<TemplateModel>>> GetAllTemplatesAsync()
        {
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

            return new Result<IEnumerable<TemplateModel>>(true, "Templates retrieved successfully.", templateDtos);
        }

        public async Task<Result<IEnumerable<TemplateFileModel>>> GetTemplateFilesByTemplateIdAsync(int templateId)
        {
            try
            {
                var templateFiles = await _templateFileRepository.GetTemplateFilesByTemplateIdAsync(templateId);
                var templateFileDtos = templateFiles.Select(tf => new TemplateFileModel
                {
                    Id = tf.Id,
                    FileName = tf.FileName,
                    FileData = tf.FileData,
                    TemplateId = tf.TemplateId
                }).ToList();

                return new Result<IEnumerable<TemplateFileModel>>(true, "Template files retrieved successfully.", templateFileDtos);
            }
            catch
            {
                return new Result<IEnumerable<TemplateFileModel>>(false, "Template not found.");
            }
        }

        public async Task<Result<IEnumerable<TemplateModel>>> GetTemplatesByUserIdAsync(int userId)
        {
            var templates = await _templateRepository.GetTemplatesByAuthorIdAsync(userId);

            if (templates == null || !templates.Any())
            {
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

            return new Result<IEnumerable<TemplateModel>>(true, "Templates retrieved successfully.", templateDtos);
        }
    }
}