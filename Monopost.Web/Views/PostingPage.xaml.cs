using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Monopost.Web.Views
{
    public partial class PostingPage : Page
    {
        private class Template
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public List<BitmapImage> Images { get; set; }
        }

        private List<Template> savedTemplates = new List<Template>();
        private int currentTemplateIndex = -1;
        private int MaxTextLength = 200;

        public PostingPage()
        {
            InitializeComponent();
            CharacterCountText.Text = $"0/{MaxTextLength}";
            TemplateNameTextBox.Visibility = Visibility.Collapsed;
            TemplateDropdown.Visibility = Visibility.Collapsed;  // Hide dropdown initially
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
                UpdateDropdownItem();  // Update dropdown when images change
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
                UpdateDropdownItem();  // Update dropdown when images change
            }
        }

        private void SaveTemplateButton_Click(object sender, RoutedEventArgs e)
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

            if (currentTemplateIndex == -1)  // Creating a new template
            {
                InputDialog inputDialog = new InputDialog();
                if (inputDialog.ShowDialog() == true)
                {
                    string templateName = inputDialog.ResponseText;

                    Template newTemplate = new Template
                    {
                        Name = templateName,
                        Text = PostTextBox.Text,
                        Images = new List<BitmapImage>()
                    };

                    foreach (BitmapImage image in ImagesControl.Items)
                    {
                        newTemplate.Images.Add(image);
                    }

                    savedTemplates.Add(newTemplate);
                    TemplateDropdown.Items.Add(newTemplate);
                    MessageBox.Show("Template saved successfully.");
                }
            }
            else  // Updating an existing template
            {
                if (string.IsNullOrWhiteSpace(TemplateNameTextBox.Text))
                {
                    MessageBox.Show("Please enter a name for the template.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var templateToUpdate = savedTemplates[currentTemplateIndex];
                templateToUpdate.Name = TemplateNameTextBox.Text;
                templateToUpdate.Text = PostTextBox.Text;
                templateToUpdate.Images.Clear();

                foreach (BitmapImage image in ImagesControl.Items)
                {
                    templateToUpdate.Images.Add(image);
                }

                UpdateDropdownItem();
                MessageBox.Show("Template updated successfully.");
            }

            TemplateDropdown.Visibility = Visibility.Collapsed;  // Hide dropdown after saving
            ClearInputFields();
        }

        private void UpdateTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            TemplateDropdown.Visibility = Visibility.Visible;
            TemplateDropdown.IsDropDownOpen = true;
        }

        private void TemplateDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TemplateDropdown.SelectedIndex >= 0 && TemplateDropdown.SelectedIndex < savedTemplates.Count)
            {
                currentTemplateIndex = TemplateDropdown.SelectedIndex;

                var selectedTemplate = savedTemplates[currentTemplateIndex];
                TemplateNameTextBox.Text = selectedTemplate.Name;
                TemplateNameTextBox.Visibility = Visibility.Visible;
                PostTextBox.Text = selectedTemplate.Text;
                UpdateCharacterCounter();

                ImagesControl.Items.Clear();
                foreach (var img in selectedTemplate.Images)
                {
                    ImagesControl.Items.Add(img);
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

        private void TemplateNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDropdownItem();
        }

        private void UpdateDropdownItem()
        {
            if (currentTemplateIndex >= 0 && currentTemplateIndex < TemplateDropdown.Items.Count)
            {
                TemplateDropdown.Items[currentTemplateIndex] = savedTemplates[currentTemplateIndex];
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
            currentTemplateIndex = -1;
        }
    }
}
