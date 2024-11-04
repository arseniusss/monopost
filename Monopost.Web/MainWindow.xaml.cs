using System.Windows;
using Monopost.Web.Views;

namespace Monopost.Web.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
            this.Content = new MainPage();
        }
    }
}

