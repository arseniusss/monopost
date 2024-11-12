using Monopost.DAL.Repositories.Interfaces;
using Monopost.Web.Commands;
using System.Windows.Controls;

namespace Monopost.Web.Views
{
    public partial class MainPage : Page
    {
        public RelayCommand NavigateProfileCommand { get; }
        public RelayCommand NavigateMonobankCommand { get; }
        public RelayCommand NavigatePostingCommand { get; }
        public RelayCommand NavigateAdminCommand { get; }

        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _postMediaRepository;

        public MainPage(ITemplateRepository templateRepository,
                        ITemplateFileRepository templateFileRepository,
                        ICredentialRepository credentialRepository,
                        IUserRepository userRepository,
                        IPostRepository postRepository,
                        IPostMediaRepository postMediaRepository)
        {
            InitializeComponent();
            DataContext = this;

            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;
            _credentialRepository = credentialRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _postMediaRepository = postMediaRepository;

            NavigateProfileCommand = new RelayCommand(_ => NavigateToProfilePage());
            NavigateMonobankCommand = new RelayCommand(_ => NavigateToMonobankPage());
            NavigatePostingCommand = new RelayCommand(_ => NavigateToPostingPage());
            NavigateAdminCommand = new RelayCommand(_ => NavigateToAdminPage());
        }

        private void NavigateToProfilePage()
        {
            MainFrame.Navigate(new ProfilePage());
        }

        private void NavigateToMonobankPage()
        {
            MainFrame.Navigate(new MonobankPage());
        }

        private void NavigateToPostingPage()
        {
            var postingPage = new PostingPage(_templateRepository, _templateFileRepository, _credentialRepository, _userRepository, _postRepository, _postMediaRepository);
            MainFrame.Navigate(postingPage);
        }

        private void NavigateToAdminPage()
        {
            MainFrame.Navigate(new AdminPage());
        }
    }
}
