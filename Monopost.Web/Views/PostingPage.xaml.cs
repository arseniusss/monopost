using Microsoft.Win32;
using Monopost.DAL.Entities;
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
    public partial class PostingPage : Page
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;

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
            }
        }

        private void AddImageToDisplay(string filePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                ImagesControl.Items.Add(bitmap);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }

        private void DeleteImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.Tag is BitmapImage imageToDelete)
            {
                ImagesControl.Items.Remove(imageToDelete);
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
                    foreach (BitmapImage image in ImagesControl.Items)
                    {
                        var fileData = ConvertImageToByteArray(image);
                        templateFiles.Add(new TemplateFile { FileName = "image", FileData = fileData });
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
                foreach (BitmapImage image in ImagesControl.Items)
                {
                    var fileData = ConvertImageToByteArray(image);
                    updatedTemplateFiles.Add(new TemplateFile { FileName = "image", FileData = fileData });
                }

                _currentTemplate.TemplateFiles = updatedTemplateFiles;
                await _templateRepository.UpdateAsync(_currentTemplate);
                MessageBox.Show("Template updated successfully.");
            }

            await LoadTemplates();
            TemplateDropdown.Visibility = Visibility.Collapsed;
            ClearInputFields();
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
                        image.BeginInit();
                        image.StreamSource = new System.IO.MemoryStream(templateFile.FileData);
                        image.EndInit();
                        ImagesControl.Items.Add(image);
                    }
                }
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
    }
}
