using System.Windows;
using Monopost.Web.ViewModels;

namespace Monopost.Web.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var mainViewModel = new MainViewModel { MainFrame = MainFrame };
            DataContext = mainViewModel;
        }
    }
}
