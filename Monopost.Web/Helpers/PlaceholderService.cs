using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Monopost.Web.Helpers
{
    public static class PlaceholderService
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.RegisterAttached("PlaceholderText", typeof(string), typeof(PlaceholderService),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public static string GetPlaceholderText(UIElement element)
        {
            return (string)element.GetValue(PlaceholderTextProperty);
        }

        public static void SetPlaceholderText(UIElement element, string value)
        {
            element.SetValue(PlaceholderTextProperty, value);
        }

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if (textBox.IsLoaded)
                {
                    ApplyPlaceholder(textBox);
                }
                else
                {
                    textBox.Loaded += (s, ev) => ApplyPlaceholder(textBox);
                }

                textBox.GotFocus += RemovePlaceholder;
                textBox.LostFocus += ApplyPlaceholder;
            }
        }

        private static void ApplyPlaceholder(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = GetPlaceholderText(textBox);
                textBox.Foreground = Brushes.Gray;
            }
        }

        private static void ApplyPlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                ApplyPlaceholder(textBox);
            }
        }

        private static void RemovePlaceholder(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Text == GetPlaceholderText(textBox))
                {
                    textBox.Text = string.Empty;
                    textBox.Foreground = Brushes.Black;
                }
            }
        }
    }
}
