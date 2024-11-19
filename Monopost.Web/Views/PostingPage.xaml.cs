using Microsoft.Win32;
using Monopost.BLL.Services;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using Monopost.PresentationLayer.Helpers;
using Serilog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Monopost.Web.Views
{
    public class ImageItem
    {
        public BitmapImage Image { get; set; }
        public string FileName { get; set; }
    }

    public partial class PostingPage : Page
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _postMediaRepository;

        public static ILogger logger = LoggerConfig.GetLogger();

        private Template _currentTemplate;
        private const int MaxTextLength = 200;

        public PostingPage(ITemplateRepository templateRepository, ITemplateFileRepository templateFileRepository,
                           ICredentialRepository credentialRepository, IUserRepository userRepository,
                           IPostRepository postRepository, IPostMediaRepository postMediaRepository)
        {
            InitializeComponent();
            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;
            _credentialRepository = credentialRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _postMediaRepository = postMediaRepository;

            CharacterCountText.Text = $"0/{MaxTextLength}";
            TemplateNameTextBox.Visibility = Visibility.Collapsed;
            TemplateDropdown.Visibility = Visibility.Collapsed;
            PostButton.Visibility = Visibility.Visible;
            InstagramCheckBox.Visibility = Visibility.Visible;
            TelegramCheckBox.Visibility = Visibility.Visible;

            LoadTemplates();
        }

        private ImageSource ConvertByteArrayToImageSource(byte[] fileData)
        {
            if (fileData == null || fileData.Length == 0)
                return null;

            var bitmap = new BitmapImage();
            using (var memoryStream = new System.IO.MemoryStream(fileData))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = memoryStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            return bitmap;
        }

        private async Task LoadTemplates()
        {
            var templates = await _templateRepository.GetAllAsync();
            templates = templates.Where(t => t.AuthorId == UserSession.GetUserId()).ToList();

            TemplateDropdown.Items.Clear();

            foreach (var template in templates)
            {
                var templateItem = new TemplateDropdownItem
                {
                    Name = template.Name,
                    PreviewImage = template.TemplateFiles.Any()
                        ? ConvertByteArrayToImageSource(template.TemplateFiles.First().FileData)
                        : null,
                    Template = template
                };

                TemplateDropdown.Items.Add(templateItem);
            }

            if (_currentTemplate != null)
            {
                var selectedTemplateItem = TemplateDropdown.Items
                    .Cast<TemplateDropdownItem>()
                    .FirstOrDefault(item => item.Template.Name == _currentTemplate.Name);

                if (selectedTemplateItem != null)
                {
                    TemplateDropdown.SelectedItem = selectedTemplateItem;
                }
            }
        }

        private async void SaveTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            if (PostTextBox.Text.Length > MaxTextLength)
            {
                MessageBox.Show($"Text exceeds the maximum allowed length of {MaxTextLength} characters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(PostTextBox.Text) && ImagesControl.Items.Count == 0)
            {
                MessageBox.Show("Please add text or an image before saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(true)
            {
                if (string.IsNullOrWhiteSpace(TemplateNameTextBox.Text))
                {
                    MessageBox.Show("Please enter a name for the template.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _currentTemplate.Name = TemplateNameTextBox.Text;
                _currentTemplate.Text = PostTextBox.Text;

                var existingFiles = await _templateFileRepository.GetTemplateFilesByTemplateIdAsync(_currentTemplate.Id);
                var updatedTemplateFiles = new List<TemplateFile>();

                foreach (ImageItem item in ImagesControl.Items)
                {
                    var fileData = ConvertImageToByteArray(item.Image);

                    var existingFile = existingFiles.FirstOrDefault(f => f.FileName == item.FileName);

                    if (existingFile == null)
                    {
                        updatedTemplateFiles.Add(new TemplateFile { FileName = item.FileName, FileData = fileData });
                    }
                    else
                    {
                        if (!existingFile.FileData.SequenceEqual(fileData))
                        {
                            existingFile.FileData = fileData;
                        }
                        updatedTemplateFiles.Add(existingFile);
                    }
                }

                var filesToDelete = existingFiles.Where(f => !updatedTemplateFiles.Any(u => u.FileName == f.FileName)).ToList();
                foreach (var file in filesToDelete)
                {
                    await _templateFileRepository.DeleteAsync(file.Id);
                }

                _currentTemplate.TemplateFiles = updatedTemplateFiles;
                await _templateRepository.UpdateAsync(_currentTemplate);
                MessageBox.Show("Template updated successfully.");
            }

            await LoadTemplates();
            TemplateDropdown.Visibility = Visibility.Collapsed;
            ClearInputFields();
        }

        private void UploadFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select an image to upload",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                AddImageToDisplay(filePath);

                PostButton.Visibility = Visibility.Visible;
                InstagramCheckBox.Visibility = Visibility.Visible;
                TelegramCheckBox.Visibility = Visibility.Visible;
            }
        }

        private void AddImageToDisplay(string filePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();

                bitmap.Freeze();

                var clonedBitmap = new BitmapImage();
                using (var memoryStream = new System.IO.MemoryStream(ConvertImageToByteArray(bitmap)))
                {
                    clonedBitmap.BeginInit();
                    clonedBitmap.StreamSource = memoryStream;
                    clonedBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    clonedBitmap.EndInit();
                    clonedBitmap.Freeze();
                }

                ImagesControl.Items.Add(new ImageItem { Image = clonedBitmap, FileName = System.IO.Path.GetFileName(filePath) });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }

        private void DeleteImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.Tag is ImageItem imageToDelete)
            {
                ImagesControl.Items.Remove(imageToDelete);
            }
        }


        private byte[] ConvertImageToByteArray(BitmapImage image)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private void UpdateTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            TemplateDropdown.Visibility = Visibility.Visible;
            TemplateDropdown.IsDropDownOpen = true;
        }

        private async void TemplateDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TemplateDropdown.SelectedIndex >= 0 && TemplateDropdown.SelectedItem is TemplateDropdownItem selectedTemplateItem)
            {
                _currentTemplate = selectedTemplateItem.Template;

                if (_currentTemplate != null)
                {
                    TemplateNameTextBox.Text = _currentTemplate.Name;
                    TemplateNameTextBox.Visibility = Visibility.Visible;
                    PostTextBox.Text = _currentTemplate.Text;
                    UpdateCharacterCounter();

                    ImagesControl.Items.Clear();
                    foreach (var templateFile in _currentTemplate.TemplateFiles)
                    {
                        BitmapImage image = new BitmapImage();
                        using (var memoryStream = new System.IO.MemoryStream(templateFile.FileData))
                        {
                            image.BeginInit();
                            image.StreamSource = memoryStream;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                        }
                        ImagesControl.Items.Add(new ImageItem { Image = image, FileName = templateFile.FileName });
                    }
                }

                PostButton.Visibility = Visibility.Visible;
                InstagramCheckBox.Visibility = Visibility.Visible;
                TelegramCheckBox.Visibility = Visibility.Visible;
            }
        }

        private void PostTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCharacterCounter();
        }

        private void UpdateCharacterCounter()
        {
            int textLength = PostTextBox.Text.Length;
            CharacterCountText.Text = $"{textLength}/{MaxTextLength}";

            if (textLength > MaxTextLength)
            {
                CharacterCountText.Foreground = Brushes.Red;
            }
            else
            {
                CharacterCountText.Foreground = Brushes.Gray;
            }
        }

        private void ClearInputFields()
        {
            TemplateNameTextBox.Clear();
            TemplateNameTextBox.Visibility = Visibility.Collapsed;
            PostTextBox.Clear();
            ImagesControl.Items.Clear();
            CharacterCountText.Text = $"0/{MaxTextLength}";
            CharacterCountText.Foreground = Brushes.Gray;
            _currentTemplate = null;
        }

        private async void PostButton_Click(object sender, RoutedEventArgs e)
        {
            string postText = PostTextBox.Text;

            List<string> filesToUpload = new List<string>();
            string imagesFolderPath = AppDomain.CurrentDomain.BaseDirectory;

            foreach (ImageItem item in ImagesControl.Items)
            {
                string filePath = System.IO.Path.Combine(imagesFolderPath, item.FileName);
                filesToUpload.Add(filePath);

                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    BitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(item.Image));
                    encoder.Save(fileStream);
                }
            }

            if (string.IsNullOrWhiteSpace(postText) && filesToUpload.Count == 0)
            {
                MessageBox.Show("Please add text or an image before posting.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<int> selectedSocialMedia = GetSelectedSocialMedia();
            if (selectedSocialMedia.Count == 0)
            {
                MessageBox.Show("Please select at least one social media platform to post to.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var postingService = new SocialMediaPostingService(
                    _credentialRepository, _userRepository, _postRepository, _postMediaRepository, 0);

                var result = await postingService.CreatePostAsync(postText, filesToUpload, selectedSocialMedia);

                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured while posting : {ex.Message} {ex.InnerException}");
                MessageBox.Show($"An error occurred while posting: {ex.Message} {ex.InnerException}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private List<int> GetSelectedSocialMedia()
        {
            List<int> selectedSocialMedia = new List<int>();

            if (InstagramCheckBox.IsChecked == true && TelegramCheckBox.IsChecked == true)
            {
                selectedSocialMedia.Add(0);
                selectedSocialMedia.Add(1);
            }
            else if (InstagramCheckBox.IsChecked == true || TelegramCheckBox.IsChecked == true)
            {
                selectedSocialMedia.Add(0);
            }

            return selectedSocialMedia;
        }

        private void InstagramCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
