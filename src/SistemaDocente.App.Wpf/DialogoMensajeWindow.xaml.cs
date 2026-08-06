using System.Windows;

namespace SistemaDocente.App.Wpf;

public enum DialogoResultado
{
    Afirmativo,
    Negativo,
    Cancelar,
}

public partial class DialogoMensajeWindow : Window
{
    private DialogoResultado _resultado = DialogoResultado.Cancelar;

    public DialogoMensajeWindow()
    {
        InitializeComponent();
    }

    public string Titulo { get; set; } = "Sistema Docente Local";
    public string Mensaje { get; set; } = string.Empty;
    public DialogoBotones Botones { get; set; } = DialogoBotones.OK;
    public DialogoIcono Icono { get; set; } = DialogoIcono.Information;

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        DataContext = this;
        AplicarConfiguracion();
    }

    private void AplicarConfiguracion()
    {
        switch (Botones)
        {
            case DialogoBotones.OK:
                BtnAfirmativo.Content = "Aceptar";
                BtnNegativo.Visibility = Visibility.Collapsed;
                BtnCancelar.Visibility = Visibility.Collapsed;
                break;
            case DialogoBotones.OKCancel:
                BtnAfirmativo.Content = "Aceptar";
                BtnNegativo.Visibility = Visibility.Collapsed;
                BtnCancelar.Content = "Cancelar";
                break;
            case DialogoBotones.YesNo:
                BtnAfirmativo.Content = "Sí";
                BtnNegativo.Content = "No";
                BtnCancelar.Visibility = Visibility.Collapsed;
                break;
            case DialogoBotones.YesNoCancel:
                BtnAfirmativo.Content = "Sí";
                BtnNegativo.Content = "No";
                BtnCancelar.Content = "Cancelar";
                break;
        }

        switch (Icono)
        {
            case DialogoIcono.Error:
                IconBorder.Background = (System.Windows.Media.Brush)FindResource("ErrorBackgroundBrush");
                IconBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("ErrorBorderBrush");
                IconText.Foreground = (System.Windows.Media.Brush)FindResource("ErrorBrush");
                IconText.Text = "❌";
                break;
            case DialogoIcono.Warning:
                IconBorder.Background = (System.Windows.Media.Brush)FindResource("WarningBackgroundBrush");
                IconBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("WarningBorderBrush");
                IconText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                IconText.Text = "⚠️";
                break;
            case DialogoIcono.Question:
                IconBorder.Background = (System.Windows.Media.Brush)FindResource("InfoBackgroundBrush");
                IconBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("InfoBorderBrush");
                IconText.Foreground = (System.Windows.Media.Brush)FindResource("InfoBrush");
                IconText.Text = "❓";
                break;
            default:
                IconBorder.Background = (System.Windows.Media.Brush)FindResource("InfoBackgroundBrush");
                IconBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("InfoBorderBrush");
                IconText.Foreground = (System.Windows.Media.Brush)FindResource("InfoBrush");
                IconText.Text = "ℹ️";
                break;
        }
    }

    private void OnAfirmativoClick(object sender, RoutedEventArgs e)
    {
        _resultado = DialogoResultado.Afirmativo;
        DialogResult = Botones == DialogoBotones.OK || Botones == DialogoBotones.OKCancel ||
                      Botones == DialogoBotones.YesNo || Botones == DialogoBotones.YesNoCancel;
        Close();
    }

    private void OnNegativoClick(object sender, RoutedEventArgs e)
    {
        _resultado = DialogoResultado.Negativo;
        DialogResult = false;
        Close();
    }

    private void OnCancelarClick(object sender, RoutedEventArgs e)
    {
        _resultado = DialogoResultado.Cancelar;
        DialogResult = false;
        Close();
    }

    public new DialogoResultado ShowDialog()
    {
        base.ShowDialog();
        return _resultado;
    }

    public static DialogoResultado Mostrar(
        string titulo,
        string mensaje,
        DialogoBotones botones = DialogoBotones.OK,
        DialogoIcono icono = DialogoIcono.Information,
        Window? owner = null)
    {
        var ventana = new DialogoMensajeWindow
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Botones = botones,
            Icono = icono,
            Owner = owner ?? System.Windows.Application.Current?.MainWindow,
        };

        return ventana.ShowDialog();
    }
}

public enum DialogoBotones
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel,
}

public enum DialogoIcono
{
    Information,
    Question,
    Warning,
    Error,
}
