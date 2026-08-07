using System.Windows;

using SistemaDocente.Presentation;

namespace SistemaDocente.App.Wpf;

public sealed class ServicioMensajesWpf : IServicioMensajes
{
    public void MostrarError(string mensaje) => DialogoMensajeWindow.Mostrar(
        "Error", mensaje, DialogoBotones.OK, DialogoIcono.Error);
}

public sealed class ServicioConfirmacionWpf : IServicioConfirmacion
{
    public bool ConfirmarDesactivacion(string nombreEstudiante) =>
        DialogoMensajeWindow.Mostrar(
            "Confirmar desactivación",
            $"¿Deseas desactivar a {nombreEstudiante}? Sus datos se conservarán.",
            DialogoBotones.YesNo,
            DialogoIcono.Question) == DialogoResultado.Afirmativo;
}

public sealed class DialogoCambiosPendientesWpf : IDialogoCambiosPendientes
{
    public DecisionCambiosPendientes ConfirmarCambiosPendientes() =>
        ConfirmarCambiosPendientes("cambios");

    public DecisionCambiosPendientes ConfirmarCambiosPendientes(string contexto)
    {
        var resultado = DialogoMensajeWindow.Mostrar(
            "Cambios sin guardar",
            $"Hay cambios pendientes en {contexto}. ¿Deseas guardarlos antes de continuar?\n\nSí: Guardar  ·  No: Descartar  ·  Cancelar: Permanecer aquí",
            DialogoBotones.YesNoCancel,
            DialogoIcono.Question);

        return resultado switch
        {
            DialogoResultado.Afirmativo => DecisionCambiosPendientes.Guardar,
            DialogoResultado.Negativo => DecisionCambiosPendientes.Descartar,
            _ => DecisionCambiosPendientes.Cancelar,
        };
    }
}

public sealed class RelojLocalSistema : IRelojLocal
{
    public DateOnly Hoy => DateOnly.FromDateTime(DateTime.Now);
}

public sealed class ConfirmacionProyectosWpf : IConfirmacionProyectos
{
    public bool Confirmar(string mensaje) =>
        DialogoMensajeWindow.Mostrar(
            "Sistema Docente Local",
            mensaje,
            DialogoBotones.YesNo,
            DialogoIcono.Question) == DialogoResultado.Afirmativo;
}