using Monopost.DAL.Repositories.Interfaces;
using Monopost.Web.Views;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using RelayCommand = Monopost.Web.Commands.RelayCommand;

namespace Monopost.Web.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _postMediaRepository;

        public Frame MainFrame { get; private set; }

        // Selected tab index (0, 1, 2, 3)
        private int _selectedTab;

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged(nameof(SelectedTab));
                }
            }
        }

        public ICommand NavigateProfileCommand { get; }
        public ICommand NavigateMonobankCommand { get; }
        public ICommand NavigatePostingCommand { get; }
        public ICommand NavigateAdminCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainViewModel(Frame mainFrame,
                             ITemplateRepository templateRepository,
                             ITemplateFileRepository templateFileRepository,
                             ICredentialRepository credentialRepository,
                             IUserRepository userRepository,
                             IPostRepository postRepository,
                             IPostMediaRepository postMediaRepository)
        {
            MainFrame = mainFrame;
            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;
            _credentialRepository = credentialRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _postMediaRepository = postMediaRepository;

            NavigateProfileCommand = new RelayCommand(_ => MainFrame.Navigate(new ProfilePage()));
            NavigateMonobankCommand = new RelayCommand(_ => MainFrame.Navigate(new MonobankPage()));
            NavigatePostingCommand = new RelayCommand(_ => MainFrame.Navigate(new PostingPage(
                _templateRepository, _templateFileRepository, _credentialRepository, _userRepository, _postRepository, _postMediaRepository)));
            NavigateAdminCommand = new RelayCommand(_ => MainFrame.Navigate(new AdminPage()));
        }
    }
}
