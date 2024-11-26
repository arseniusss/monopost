using Microsoft.Win32;
using Monopost.BLL.Services;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.Logging;
using Monopost.BLL.Helpers;
using Serilog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Monopost.BLL.SocialMediaManagement.Posting;


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
            TemplateNameTextBox.Visibility = Visibility.Visible;
            TemplateDropdown.Visibility = Visibility.Collapsed;
            PostButton.Visibility = Visibility.Visible;
            InstagramCheckBox.Visibility = Visibility.Visible;
            TelegramCheckBox.Visibility = Visibility.Visible;
            ClearButton.Visibility = Visibility.Collapsed;


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
            TemplateDropdown.IsEnabled = false; 
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

            TemplateDropdown.IsEnabled = true; 
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

            if (_currentTemplate == null)
            {
                if (string.IsNullOrWhiteSpace(TemplateNameTextBox.Text))
                {
                    MessageBox.Show("Please enter a template name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _currentTemplate = new Template
                {
                    Name = TemplateNameTextBox.Text,  
                    Text = PostTextBox.Text,
                    TemplateFiles = new List<TemplateFile>(),
                    AuthorId = UserSession.GetUserId()
                };
            }
            else
            {
                _currentTemplate.Name = TemplateNameTextBox.Text;
                _currentTemplate.Text = PostTextBox.Text;
            }

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

            if (_currentTemplate.Id == 0)
            {
                await _templateRepository.AddAsync(_currentTemplate);
            }
            else
            {
                await _templateRepository.UpdateAsync(_currentTemplate);
            }

            MessageBox.Show("Template saved successfully.");

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

                UpdateClearButtonVisibility();  
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
            if (e.AddedItems.Count == 0) return; 

            if (TemplateDropdown.SelectedItem is TemplateDropdownItem selectedTemplateItem)
            {
                if (selectedTemplateItem.Template == null)
                {
                    MessageBox.Show("Template data not loaded. Please try again.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _currentTemplate = selectedTemplateItem.Template;

                TemplateNameTextBox.Text = _currentTemplate.Name;
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
        }


        private void PostTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCharacterCounter();
            UpdateClearButtonVisibility(); 
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
        private void UpdateClearButtonVisibility()
        {
            bool isFormNotEmpty = !string.IsNullOrWhiteSpace(PostTextBox.Text) && PostTextBox.Text != "Enter text here" || ImagesControl.Items.Count > 1;

            ClearButton.Visibility = isFormNotEmpty ? Visibility.Visible : Visibility.Collapsed;
        }


        private void ClearInputFields()
        {
            TemplateNameTextBox.Clear();
            TemplateNameTextBox.Visibility = Visibility.Visible;
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

            try
            {
                var postingService = new SocialMediaPostingService(
                    _credentialRepository, _userRepository, _postRepository, _postMediaRepository, UserSession.GetUserId());

                await postingService.AddPosters();

                List<int> selectedSocialMedia = GetSelectedSocialMedia(_socialMediaPostersService: postingService);
                if (selectedSocialMedia.Count == 0)
                {
                    MessageBox.Show("Please select at least one social media platform to post to. You may not have the valid data for your selected poster", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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


        private List<int> GetSelectedSocialMedia(SocialMediaPostingService _socialMediaPostersService)
        {
            List<int> selectedSocialMedia = new List<int>();

            int instagramIndex = _socialMediaPostersService._socialMediaPosters.FindIndex(poster => poster is InstagramPoster);
            int telegramIndex = _socialMediaPostersService._socialMediaPosters.FindIndex(poster => poster is TelegramPoster);

            if (InstagramCheckBox.IsChecked == true && instagramIndex >= 0)
            {
                selectedSocialMedia.Add(instagramIndex);
            }

            if (TelegramCheckBox.IsChecked == true && telegramIndex >= 0)
            {
                selectedSocialMedia.Add(telegramIndex);
            }

            return selectedSocialMedia;
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PostTextBox.Text = "Enter text here";
            PostTextBox.Foreground = Brushes.Gray;

            TemplateNameTextBox.Text = "Enter text here";
            TemplateNameTextBox.Foreground = Brushes.Gray;

            ImagesControl.Items.Clear(); 

            CharacterCountText.Text = $"0/{MaxTextLength}";
            CharacterCountText.Foreground = Brushes.Gray;

            TemplateNameTextBox.Visibility = Visibility.Visible;

            _currentTemplate = null;

            ClearButton.Visibility = Visibility.Collapsed;

            PostButton.Visibility = Visibility.Collapsed;
            InstagramCheckBox.Visibility = Visibility.Collapsed;
            TelegramCheckBox.Visibility = Visibility.Collapsed;
        }
        private void InstagramCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
