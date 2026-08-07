using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SistemaDocente.App.Wpf.Controls
{
    public partial class EmptyState : UserControl
    {
        public EmptyState()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(EmptyState),
                new PropertyMetadata(string.Empty, OnIconChanged));

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(EmptyState),
                new PropertyMetadata(string.Empty, OnTitleChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(EmptyState),
                new PropertyMetadata(string.Empty, OnMessageChanged));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty ActionTextProperty =
            DependencyProperty.Register(nameof(ActionText), typeof(string), typeof(EmptyState),
                new PropertyMetadata(string.Empty, OnActionTextChanged));

        public string ActionText
        {
            get => (string)GetValue(ActionTextProperty);
            set => SetValue(ActionTextProperty, value);
        }

        public static readonly DependencyProperty ActionCommandProperty =
            DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(EmptyState),
                new PropertyMetadata(null, OnActionCommandChanged));

        public ICommand ActionCommand
        {
            get => (ICommand)GetValue(ActionCommandProperty);
            set => SetValue(ActionCommandProperty, value);
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmptyState control && control.IconText != null)
            {
                control.IconText.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmptyState control && control.TitleText != null)
            {
                control.TitleText.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmptyState control && control.MessageText != null)
            {
                control.MessageText.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private static void OnActionTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmptyState control && control.ActionButton != null)
            {
                var text = e.NewValue as string;
                control.ActionButton.Content = text;
                control.ActionButton.Visibility = string.IsNullOrWhiteSpace(text) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private static void OnActionCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EmptyState control && control.ActionButton != null)
            {
                control.ActionButton.Command = e.NewValue as ICommand;
            }
        }
    }
}