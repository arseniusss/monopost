using Microsoft.Extensions.DependencyInjection;
using Monopost.DAL.Repositories.Interfaces;
using System.Windows;

namespace Monopost.Web.Views
{
    public partial class MainWindow : Window
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly IPostMediaRepository _postMediaRepository;

        public MainWindow()
        {
            InitializeComponent();

            _templateRepository = App.ServiceProvider.GetRequiredService<ITemplateRepository>();
            _templateFileRepository = App.ServiceProvider.GetRequiredService<ITemplateFileRepository>();
            _credentialRepository = App.ServiceProvider.GetRequiredService<ICredentialRepository>();
            _userRepository = App.ServiceProvider.GetRequiredService<IUserRepository>();
            _postRepository = App.ServiceProvider.GetRequiredService<IPostRepository>();
            _postMediaRepository = App.ServiceProvider.GetRequiredService<IPostMediaRepository>();


            this.Content = new LoginPage(this, _userRepository);
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        public void NavigateToMainContent()
        {
            var mainPage = new MainPage(_templateRepository, _templateFileRepository, _credentialRepository, _userRepository, _postRepository, _postMediaRepository);
            this.Content = mainPage;
        }

        public void NavigateToRegisterPage()
        {
            var registerPage = new RegisterPage(this,_userRepository);
            this.Content = registerPage;
        }

        public void NavigateToLogInPage()
        {
            var logInPage = new LoginPage(this, _userRepository);
            this.Content = logInPage;
        }

        //public void NavigateToReset()
        //{
        //    var resetPage = new ResetPasswordPage()
        //}
        
    }
}
