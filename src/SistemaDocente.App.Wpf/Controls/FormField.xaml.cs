using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace SistemaDocente.App.Wpf.Controls;

[ContentProperty(nameof(FieldContent))]
public partial class FormField : UserControl
{
    public FormField()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(FormField),
            new PropertyMetadata(string.Empty));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty FieldContentProperty =
        DependencyProperty.Register(
            nameof(FieldContent),
            typeof(object),
            typeof(FormField),
            new PropertyMetadata(null));

    public object? FieldContent
    {
        get => GetValue(FieldContentProperty);
        set => SetValue(FieldContentProperty, value);
    }

    public static readonly DependencyProperty ErrorProperty =
        DependencyProperty.Register(
            nameof(Error),
            typeof(string),
            typeof(FormField),
            new PropertyMetadata(string.Empty, OnErrorChanged));

    public string Error
    {
        get => (string)GetValue(ErrorProperty);
        set => SetValue(ErrorProperty, value);
    }

    private static void OnErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FormField control && control.ErrorText is not null)
        {
            var error = e.NewValue as string;
            control.ErrorText.Visibility = string.IsNullOrWhiteSpace(error)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}
