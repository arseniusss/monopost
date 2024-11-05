using Monopost.DAL.Repositories.Interfaces;
using Monopost.Web.Commands;
using Monopost.Web.Views;
using System.Windows.Controls;
using System.Windows.Input;

namespace Monopost.Web.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly IUserRepository _userRepository;
        public Frame MainFrame { get; private set; }

        public ICommand NavigateProfileCommand { get; }
        public ICommand NavigateMonobankCommand { get; }
        public ICommand NavigatePostingCommand { get; }
        public ICommand NavigateAdminCommand { get; }

        public MainViewModel(Frame mainFrame, ITemplateRepository templateRepository, ITemplateFileRepository templateFileRepository)
        {
            MainFrame = mainFrame;
            _templateRepository = templateRepository;
            _templateFileRepository = templateFileRepository;

            NavigateProfileCommand = new RelayCommand(_ => MainFrame.Navigate(new ProfilePage()));
            NavigateMonobankCommand = new RelayCommand(_ => MainFrame.Navigate(new MonobankPage()));
            NavigatePostingCommand = new RelayCommand(_ => MainFrame.Navigate(new PostingPage(_templateRepository, _templateFileRepository, _userRepository)));
            NavigateAdminCommand = new RelayCommand(_ => MainFrame.Navigate(new AdminPage()));
        }
    }
}
