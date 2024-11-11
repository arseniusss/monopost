using Monopost.DAL.Repositories.Interfaces;
using Monopost.Web.Commands;
using System.Windows.Controls;
using System.Windows.Input;
using Monopost.Web.Views;
using RelayCommand = Monopost.Web.Commands.RelayCommand;

namespace Monopost.Web.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;

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
            NavigatePostingCommand = new RelayCommand(_ => MainFrame.Navigate(new PostingPage(_templateRepository, _templateFileRepository)));
            NavigateAdminCommand = new RelayCommand(_ => MainFrame.Navigate(new AdminPage()));
        }
    }
}
