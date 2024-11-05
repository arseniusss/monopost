using Microsoft.Extensions.DependencyInjection;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.Web.Views;
using System.Windows;

namespace Monopost.Web.Views
{
    public partial class MainWindow : Window
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateFileRepository _templateFileRepository;
        private readonly IUserRepository _userRepository;

        public MainWindow()
        {
            InitializeComponent();

            _templateRepository = App.ServiceProvider.GetRequiredService<ITemplateRepository>();
            _templateFileRepository = App.ServiceProvider.GetRequiredService<ITemplateFileRepository>();
            _userRepository = App.ServiceProvider.GetRequiredService<IUserRepository>();

            this.Content = new LoginPage(this);
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public void NavigateToMainContent()
        {
            var mainPage = new MainPage(_templateRepository, _templateFileRepository, _userRepository);
            this.Content = mainPage;
        }
    }
}
