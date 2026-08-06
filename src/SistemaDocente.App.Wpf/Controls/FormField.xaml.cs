using System.Windows;
using System.Windows.Controls;

namespace SistemaDocente.App.Wpf.Controls
{
    public partial class FormField : UserControl
    {
        public FormField()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(FormField),
                new PropertyMetadata(string.Empty, OnHeaderChanged));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(nameof(Error), typeof(string), typeof(FormField),
                new PropertyMetadata(string.Empty, OnErrorChanged));

        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FormField control && control.HeaderText != null)
            {
                control.HeaderText.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private static void OnErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FormField control && control.ErrorText != null)
            {
                var error = e.NewValue as string;
                control.ErrorText.Text = error ?? string.Empty;
                control.ErrorText.Visibility = string.IsNullOrWhiteSpace(error) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
