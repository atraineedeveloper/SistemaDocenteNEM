using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;

using Microsoft.Win32;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf.Views;

public partial class ReportesView : UserControl
{
    public ReportesView()
    {
        InitializeComponent();
        AgregarAccionPdf();
    }

    public static readonly DependencyProperty ConfiguracionProperty = DependencyProperty.Register(
        nameof(Configuracion),
        typeof(ConfiguracionGrupoViewModel),
        typeof(ReportesView),
        new PropertyMetadata(null));

    public ConfiguracionGrupoViewModel? Configuracion
    {
        get => (ConfiguracionGrupoViewModel?)GetValue(ConfiguracionProperty);
        set => SetValue(ConfiguracionProperty, value);
    }

    private GestionReportesViewModel? ViewModel => DataContext as GestionReportesViewModel;

    private void OnConfigurarGrupoClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.GrupoIdActual is not { } grupoId || Configuracion is null) return;
        Configuracion.Inicializar(grupoId);
        var ventana = new ConfiguracionGrupoWindow(Configuracion)
        {
            Owner = Window.GetWindow(this),
        };
        ventana.ShowDialog();
        ViewModel.Refrescar();
    }

    private void OnGuardarPdfClic(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { PuedeExportarPdf: true } viewModel) return;
        var owner = Window.GetWindow(this);
        var confirmacion = MessageBox.Show(
            owner,
            viewModel.AdvertenciaPdf + "\n\n¿Deseas continuar y elegir dónde guardar el archivo?",
            "Guardar reporte PDF",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (confirmacion != MessageBoxResult.Yes) return;

        var dialogo = new SaveFileDialog
        {
            Title = "Guardar reporte PDF de AulaRaíz",
            Filter = "Documento PDF (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = viewModel.CrearNombrePdfSugerido(DateOnly.FromDateTime(DateTime.Today)),
        };
        if (dialogo.ShowDialog(owner) != true) return;

        if (viewModel.ExportarPdf(dialogo.FileName))
        {
            MessageBox.Show(
                owner,
                "El reporte PDF se guardó correctamente.",
                "PDF creado",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        MessageBox.Show(
            owner,
            viewModel.Mensaje,
            "No fue posible guardar el PDF",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void AgregarAccionPdf()
    {
        var botonConfiguracion = EncontrarDescendiente<Button>(
            this,
            boton => string.Equals(
                AutomationProperties.GetName(boton),
                "Abrir configuración del grupo",
                StringComparison.Ordinal));
        if (botonConfiguracion?.Parent is not StackPanel acciones) return;

        var botonPdf = new Button
        {
            Content = "▣  Guardar PDF",
            MinWidth = 120,
        };
        botonPdf.SetResourceReference(StyleProperty, typeof(Button));
        botonPdf.SetBinding(
            IsEnabledProperty,
            new Binding(nameof(GestionReportesViewModel.PuedeExportarPdf)));
        AutomationProperties.SetName(botonPdf, "Guardar reporte actual como PDF");
        botonPdf.Click += OnGuardarPdfClic;
        acciones.Children.Insert(Math.Max(0, acciones.Children.IndexOf(botonConfiguracion)), botonPdf);
    }

    private static T? EncontrarDescendiente<T>(DependencyObject raiz, Func<T, bool> criterio)
        where T : DependencyObject
    {
        foreach (var hijo in LogicalTreeHelper.GetChildren(raiz).OfType<DependencyObject>())
        {
            if (hijo is T candidato && criterio(candidato)) return candidato;
            var descendiente = EncontrarDescendiente(hijo, criterio);
            if (descendiente is not null) return descendiente;
        }
        return null;
    }
}