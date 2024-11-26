using Microsoft.Win32;
using Monopost.BLL.Helpers;
using Monopost.BLL.Models;
using Monopost.BLL.Services.Implementations;
using Monopost.BLL.Services.Interfaces;
using Monopost.DAL.Repositories.Interfaces;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Monopost.Web.Views
{
    public partial class ProfilePage : Page
    {
        private readonly MainWindow _mainWindow;
        private readonly IUserPersonalInfoManagementService _userPersonalInfoManagementService;
        private readonly IDataExtractionService _dataExtractionService;
        private readonly IUserRepository _userRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _postMediaRepository;
        private UserPersonalInfoModel _originalUserData;
        //private UserPersonalInfoModel _savedUserData;
        private readonly IDataDeletionService _dataDeletionService;

        public ProfilePage(
            IUserRepository userRepository,
            ICredentialRepository credentialRepository,
            ITemplateRepository templateRepository,
            ITemplateFileRepository templateFileRepository,
            IPostRepository postRepository,
            IPostMediaRepository postMediaRepository)
        {
            InitializeComponent();
            _userRepository = userRepository;
            _credentialRepository = credentialRepository;
            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;
            _postRepository = postRepository;
            _postMediaRepository = postMediaRepository;

            _userPersonalInfoManagementService = new UserPersonalInfoManagementService(userRepository);
            _dataExtractionService = new DataExtractionService(
                userRepository,
                credentialRepository,
                templateRepository,
                templateFileRepository,
                postRepository,
                postMediaRepository
            );

            _dataDeletionService = new DataDeletionService(
               userRepository,
               credentialRepository,
               templateRepository,
               templateFileRepository,
               postRepository,
               postMediaRepository
           );

            ExtractButton.Click += ExtractButton_Click;
            DeleteButton.Click += DeleteButton_Click;

            LoadUserData();
        }


        private async void LoadUserData()
        {
            if (NameTextBox == null || LastNameTextBox == null || AgeTextBox == null)
            {
                MessageBox.Show("UI elements are not initialized", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int id = UserSession.GetUserId();
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                MessageBox.Show("User not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _originalUserData = new UserPersonalInfoModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Age = user.Age
            };



            NameTextBox.Text = _originalUserData.FirstName;
            LastNameTextBox.Text = _originalUserData.LastName;
            AgeTextBox.Text = _originalUserData.Age.ToString();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var isDataValid = !string.IsNullOrWhiteSpace(NameTextBox.Text) &&
                            !string.IsNullOrWhiteSpace(LastNameTextBox.Text) &&
                            int.TryParse(AgeTextBox.Text, out int age) &&
                            age > 0 && age < 120;

            var hasChanges = NameTextBox.Text != _originalUserData.FirstName ||
                           LastNameTextBox.Text != _originalUserData.LastName ||
                           AgeTextBox.Text != _originalUserData.Age.ToString();

            UpdateButton.IsEnabled = isDataValid && hasChanges;
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(AgeTextBox.Text, out int age))
            {
                MessageBox.Show("Please enter a valid age", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var model = new UserPersonalInfoModel
            {
                Id = _originalUserData.Id,
                FirstName = NameTextBox.Text,
                LastName = LastNameTextBox.Text,
                Age = age
            };

            var result = await _userPersonalInfoManagementService.UpdateUserPersonalInfoAsync(model);

            if (result.Success)
            {
                _originalUserData = model;
                UpdateButton.IsEnabled = false;
                MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            NameTextBox.Text = _originalUserData.FirstName;
            LastNameTextBox.Text = _originalUserData.LastName;
            AgeTextBox.Text = _originalUserData.Age.ToString();
            UpdateButton.IsEnabled = false;
        }

        private async void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentUserId = UserSession.GetUserId();

                var extractionResult = await _dataExtractionService.ExtractData(
                    currentUserId,
                    includeCredentials: CredentialsTextBox.IsChecked ?? false,
                    includeTemplates: TemplatesTextBox.IsChecked ?? false,
                    includePosts: PostsTextBox.IsChecked ?? false,
                    totalDataExtraction: false
                );

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    DefaultExt = "json",
                    Title = "Save Extracted Data",
                    FileName = $"extracted_data_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var saveResult = _dataExtractionService.SaveResultToJson(extractionResult, saveFileDialog.FileName);

                    if (saveResult.Success)
                    {
                        MessageBox.Show(
                            "Data extracted and saved successfully!",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Failed to save data: {saveResult.Message}",
                            "Save Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete selected data? This action cannot be undone.",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int currentUserId = UserSession.GetUserId();

                    var deletionResult = await _dataDeletionService.DeleteData(
                        currentUserId,
                        credentials: CredentialsTextBox.IsChecked ?? false,
                        templates: TemplatesTextBox.IsChecked ?? false,
                        posts: PostsTextBox.IsChecked ?? false,
                        totalAccountDeletion: false
                    );

                    if (deletionResult.Success)
                    {
                        MessageBox.Show(
                            "Selected data has been successfully deleted!",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );

                        if (CredentialsTextBox.IsChecked == true ||
                            TemplatesTextBox.IsChecked == true ||
                            PostsTextBox.IsChecked == true)
                        {
                            LoadUserData();
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Failed to delete data: {deletionResult.Message}",
                            "Delete Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"An error occurred while deleting data: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }


    }
}
