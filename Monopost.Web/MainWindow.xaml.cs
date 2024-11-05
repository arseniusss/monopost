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

        public MainWindow()
        {
            InitializeComponent();

            _templateRepository = App.ServiceProvider.GetRequiredService<ITemplateRepository>();
            _templateFileRepository = App.ServiceProvider.GetRequiredService<ITemplateFileRepository>();

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
            var mainPage = new MainPage(_templateRepository, _templateFileRepository);
            this.Content = mainPage;
        }
    }
}
