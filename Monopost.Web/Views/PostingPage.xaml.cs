using Microsoft.Win32;
using Monopost.BLL.Services;
using Monopost.DAL.Entities;
using Monopost.DAL.Repositories.Implementations;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.PresentationLayer.Helpers;
using Monopost.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private Template _currentTemplate;
        private const int MaxTextLength = 200;

        public PostingPage(ITemplateRepository templateRepository, ITemplateFileRepository templateFileRepository)
        {
            InitializeComponent();
            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;
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
            templates = templates.Where(t => t.AuthorId == UserSession.CurrentUserId).ToList();

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

            if (_currentTemplate == null)
            {
                InputDialog inputDialog = new InputDialog();
                if (inputDialog.ShowDialog() == true)
                {
                    string templateName = inputDialog.ResponseText;

                    Template newTemplate = new Template
                    {
                        Name = templateName,
                        Text = PostTextBox.Text,
                        AuthorId = UserSession.CurrentUserId
                    };

                    var templateFiles = new List<TemplateFile>();
                    foreach (ImageItem item in ImagesControl.Items)
                    {
                        var fileData = ConvertImageToByteArray(item.Image);
                        templateFiles.Add(new TemplateFile { FileName = item.FileName, FileData = fileData });
                    }

                    newTemplate.TemplateFiles = templateFiles;
                    await _templateRepository.AddAsync(newTemplate);
                    MessageBox.Show("Template saved successfully.");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(TemplateNameTextBox.Text))
                {
                    MessageBox.Show("Please enter a name for the template.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _currentTemplate.Name = TemplateNameTextBox.Text;
                _currentTemplate.Text = PostTextBox.Text;

                var existingFiles = await _templateFileRepository.GetTemplateFilesByTemplateIdAsync(_currentTemplate.Id);
                foreach (var file in existingFiles)
                {
                    await _templateFileRepository.DeleteAsync(file.Id);
                }

                var updatedTemplateFiles = new List<TemplateFile>();
                foreach (ImageItem item in ImagesControl.Items)
                {
                    var fileData = ConvertImageToByteArray(item.Image);
                    updatedTemplateFiles.Add(new TemplateFile { FileName = item.FileName, FileData = fileData });
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

                ImagesControl.Items.Add(new ImageItem { Image = bitmap, FileName = System.IO.Path.GetFileName(filePath) });
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
            foreach (ImageItem item in ImagesControl.Items)
            {
                filesToUpload.Add(item.FileName);
            }

            if (string.IsNullOrWhiteSpace(postText) && filesToUpload.Count == 0)
            {
                MessageBox.Show("Please add text or an image before posting.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<int> selectedSocialMedia = GetSelectedSocialMedia(); 

            var postingService = new SocialMediaPostingService(
                _credentialRepository, _userRepository, _postRepository, _postMediaRepository, UserSession.CurrentUserId);

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

        private List<int> GetSelectedSocialMedia()
        {
            List<int> selectedSocialMedia = new List<int>();

            if (InstagramCheckBox.IsChecked == true)
            {
                selectedSocialMedia.Add(0);
            }
            if (TelegramCheckBox.IsChecked == true)
            {
                selectedSocialMedia.Add(1); 
            }

            return selectedSocialMedia;
        }

        private void InstagramCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
