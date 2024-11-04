using System.Windows;

namespace Monopost.Web.Views
{
    public partial class InputDialog : Window
    {
        public string ResponseText
        {
            get => InputTextBox.Text;
            set => InputTextBox.Text = value;
        }

        public InputDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
