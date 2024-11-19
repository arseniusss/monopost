using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Monopost.PresentationLayer.Helpers
{
    public class ClearableTextBox : TextBox
    {
        public ClearableTextBox()
        {
            this.GotFocus += RemoveText;
            this.LostFocus += AddText;
        }

        public string PlaceholderText { get; set; }

        private void RemoveText(object sender, RoutedEventArgs e)
        {
            if (this.Text == PlaceholderText)
            {
                this.Text = string.Empty;
                this.Foreground = Brushes.Black;
            }
        }

        private void AddText(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Text))
            {
                this.Text = PlaceholderText;
                this.Foreground = Brushes.Gray;
            }
        }
    }
}