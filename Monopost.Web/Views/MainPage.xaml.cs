using Monopost.Web.Commands;
using Monopost.DAL.Repositories.Interfaces;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;  
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

        public MainPage(ITemplateRepository templateRepository, ITemplateFileRepository templateFileRepository)
        {
            InitializeComponent();
            DataContext = this;

            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;

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
            var postingPage = new PostingPage(_templateRepository, _templateFileRepository);
            MainFrame.Navigate(postingPage);
        }

        private void NavigateToAdminPage()
        {
            MainFrame.Navigate(new AdminPage());
        }
    }
}
