using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace Monopost.Web.Views
{
    public partial class PostingPage : Page
    {
        public PostingPage()
        {
            InitializeComponent();
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

        private void SaveTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel templatePanel = new StackPanel { Margin = new Thickness(5) };

            ToggleButton toggleButton = new ToggleButton
            {
                Content = PostTextBox.Text.Length > 20 ? PostTextBox.Text.Substring(0, 20) + "..." : PostTextBox.Text,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            toggleButton.Click += (s, args) =>
            {
                if (toggleButton.IsChecked == true)
                {
                    toggleButton.Content = PostTextBox.Text;

                    foreach (BitmapImage img in ImagesControl.Items)
                    {
                        Image newImage = new Image
                        {
                            Source = img,
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(5)
                        };
                        templatePanel.Children.Add(newImage);
                    }
                }
                else
                {
                    toggleButton.Content = PostTextBox.Text.Length > 20 ? PostTextBox.Text.Substring(0, 20) + "..." : PostTextBox.Text;

                    if (ImagesControl.Items.Count > 0)
                    {
                        Image firstImage = new Image
                        {
                            Source = ((BitmapImage)ImagesControl.Items[0]),
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(5)
                        };
                        templatePanel.Children.Clear();
                        templatePanel.Children.Add(firstImage);
                    }
                }
            };

            templatePanel.Children.Add(toggleButton);
            TemplateControl.Items.Add(templatePanel);

            PostTextBox.Clear();
            ImagesControl.Items.Clear();
        }

        private void DeleteImageButton_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = sender as Button;
            if (deleteButton != null)
            {
                BitmapImage imageToDelete = deleteButton.Tag as BitmapImage;

                if (imageToDelete != null)
                {
                    ImagesControl.Items.Remove(imageToDelete);
                }
            }
        }
    }
}
