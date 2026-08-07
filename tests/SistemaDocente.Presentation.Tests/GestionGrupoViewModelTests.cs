using SistemaDocente.Application;
using SistemaDocente.Core;

namespace SistemaDocente.Presentation.Tests;

public sealed class GestionGrupoViewModelTests
{
    [Fact]
    public void PrimeraAperturaSinReferenciaMuestraBienvenida()
    {
        var contexto = new ContextoPrueba();
        contexto.ViewModel.Inicializar();
        Assert.True(contexto.ViewModel.MostrarBienvenida);
        Assert.False(contexto.ViewModel.MostrarGestion);
    }

    [Fact]
    public void ReferenciaInvalidaMuestraMensajeYBienvenida()
    {
        var contexto = new ContextoPrueba();
        contexto.Estado.Resultado = new(EstadoLecturaReferencia.Invalida);
        contexto.ViewModel.Inicializar();
        Assert.True(contexto.ViewModel.MostrarBienvenida);
        Assert.Single(contexto.Mensajes.Mensajes);
    }

    [Fact]
    public void ReferenciaValidaCargaGrupoEnOrdenRecibido()
    {
        var contexto = new ContextoPrueba();
        contexto.Estado.Resultado = new(EstadoLecturaReferencia.Valida, contexto.Gestion.Grupo.GrupoId);
        contexto.ViewModel.Inicializar();
        Assert.True(contexto.ViewModel.MostrarGestion);
        Assert.Equal(contexto.Gestion.Grupo.Estudiantes.Select(x => x.NombreVisible), contexto.ViewModel.Estudiantes.Select(x => x.Nombre));
    }

    [Fact]
    public void GrupoInexistentePermiteOlvidarReferencia()
    {
        var contexto = new ContextoPrueba();
        contexto.Estado.Resultado = new(EstadoLecturaReferencia.Valida, contexto.Gestion.Grupo.GrupoId);
        contexto.Gestion.ErrorCarga = new GrupoNoEncontradoException("ausente");
        contexto.ViewModel.Inicializar();
        Assert.True(contexto.ViewModel.PuedeOlvidarReferencia);
        contexto.ViewModel.OlvidarReferenciaCommand.Execute(null);
        Assert.True(contexto.Estado.Olvidada);
    }

    [Fact]
    public void CrearEscribeReferenciaSoloTrasExito()
    {
        var contexto = new ContextoPrueba();
        contexto.ViewModel.NombreNuevoGrupo = "Primero A";
        contexto.ViewModel.CrearGrupoCommand.Execute(null);
        Assert.Equal(contexto.Gestion.Grupo.GrupoId, contexto.Estado.Guardado);
        Assert.True(contexto.ViewModel.MostrarGestion);
    }

    [Fact]
    public void CrearInvalidoConservaEntradaYNoEscribeReferencia()
    {
        var contexto = new ContextoPrueba();
        contexto.Gestion.ErrorCrear = new DomainValidationException("Nombre inválido");
        contexto.ViewModel.NombreNuevoGrupo = "  ";
        contexto.ViewModel.CrearGrupoCommand.Execute(null);
        Assert.Equal("  ", contexto.ViewModel.NombreNuevoGrupo);
        Assert.Null(contexto.Estado.Guardado);
        Assert.Equal("Nombre inválido", contexto.ViewModel.MensajeEdicion);
    }

    [Fact]
    public void RenombrarGrupoActualizaSoloTrasExito()
    {
        var contexto = ContextoPrueba.Cargado();
        contexto.ViewModel.AbrirCambioNombreCommand.Execute(null);
        contexto.ViewModel.NombreEdicionGrupo = "Segundo B";
        contexto.ViewModel.GuardarNombreGrupoCommand.Execute(null);
        Assert.Equal("Segundo B", contexto.ViewModel.NombreGrupo);
    }

    [Fact]
    public void AltaYEdicionActualizanLaListaConfirmada()
    {
        var contexto = ContextoPrueba.Cargado();
        contexto.ViewModel.AbrirAgregarEstudianteCommand.Execute(null);
        contexto.ViewModel.NombreEstudianteEdicion = "Luis";
        contexto.ViewModel.NumeroListaEdicion = "3";
        contexto.ViewModel.GradoEdicion = GradoPrimaria.Primero;
        contexto.ViewModel.GuardarEstudianteCommand.Execute(null);
        Assert.Contains(contexto.ViewModel.Estudiantes, x => x.Nombre == "Luis");

        contexto.ViewModel.EstudianteSeleccionado = contexto.ViewModel.Estudiantes[^1];
        contexto.ViewModel.AbrirEditarEstudianteCommand.Execute(null);
        contexto.ViewModel.NombreEstudianteEdicion = "Luis Alberto";
        contexto.ViewModel.NumeroListaEdicion = "4";
        contexto.ViewModel.GradoEdicion = GradoPrimaria.Primero;
        contexto.ViewModel.GuardarEstudianteCommand.Execute(null);
        Assert.Contains(contexto.ViewModel.Estudiantes, x => x.Nombre == "Luis Alberto" && x.NumeroLista == 4);
        Assert.Equal(1, contexto.Gestion.Ediciones);
        Assert.Equal(0, contexto.Gestion.Renombrados);
        Assert.Equal(0, contexto.Gestion.CambiosNumero);
    }

    [Fact]
    public void ConflictoEnEdicionAtomicaConservaSnapshotEntradasYPanel()
    {
        var contexto = ContextoPrueba.Cargado();
        var snapshotAnterior = contexto.ViewModel.Estudiantes;
        contexto.ViewModel.EstudianteSeleccionado = contexto.ViewModel.Estudiantes[0];
        contexto.ViewModel.AbrirEditarEstudianteCommand.Execute(null);
        contexto.ViewModel.NombreEstudianteEdicion = "Nombre pendiente";
        contexto.ViewModel.NumeroListaEdicion = "2";
        contexto.ViewModel.GradoEdicion = GradoPrimaria.Primero;
        contexto.Gestion.ErrorEditar = new DomainConflictException("Número ocupado");

        contexto.ViewModel.GuardarEstudianteCommand.Execute(null);

        Assert.Same(snapshotAnterior, contexto.ViewModel.Estudiantes);
        Assert.Equal("Nombre pendiente", contexto.ViewModel.NombreEstudianteEdicion);
        Assert.Equal("2", contexto.ViewModel.NumeroListaEdicion);
        Assert.Equal("Número ocupado", contexto.ViewModel.MensajeEdicion);
        Assert.Equal(PanelEdicion.EditarEstudiante, contexto.ViewModel.PanelActual);
        Assert.Equal(1, contexto.Gestion.Ediciones);
        Assert.Equal(0, contexto.Gestion.Renombrados);
        Assert.Equal(0, contexto.Gestion.CambiosNumero);
    }

    [Fact]
    public void ConflictoConservaEntradasYEstadoAnterior()
    {
        var contexto = ContextoPrueba.Cargado();
        var anteriores = contexto.ViewModel.Estudiantes;
        contexto.Gestion.ErrorAgregar = new DomainConflictException("Número ocupado");
        contexto.ViewModel.AbrirAgregarEstudianteCommand.Execute(null);
        contexto.ViewModel.NombreEstudianteEdicion = "Luis";
        contexto.ViewModel.NumeroListaEdicion = "1";
        contexto.ViewModel.GradoEdicion = GradoPrimaria.Primero;
        contexto.ViewModel.GuardarEstudianteCommand.Execute(null);
        Assert.Equal("Luis", contexto.ViewModel.NombreEstudianteEdicion);
        Assert.Equal("1", contexto.ViewModel.NumeroListaEdicion);
        Assert.Equal("Número ocupado", contexto.ViewModel.MensajeEdicion);
        Assert.Same(anteriores, contexto.ViewModel.Estudiantes);
    }

    [Fact]
    public void CancelarEdicionNoInvocaGestion()
    {
        var contexto = ContextoPrueba.Cargado();
        contexto.ViewModel.AbrirAgregarEstudianteCommand.Execute(null);
        contexto.ViewModel.CancelarEdicionCommand.Execute(null);
        Assert.Equal(0, contexto.Gestion.Altas);
        Assert.Equal(PanelEdicion.Ninguno, contexto.ViewModel.PanelActual);
    }

    [Fact]
    public void DesactivarRequiereConfirmacionYReactivarActualiza()
    {
        var contexto = ContextoPrueba.Cargado();
        contexto.ViewModel.EstudianteSeleccionado = contexto.ViewModel.Estudiantes[0];
        contexto.Confirmacion.Respuesta = false;
        contexto.ViewModel.DesactivarEstudianteCommand.Execute(null);
        Assert.Equal(0, contexto.Gestion.Desactivaciones);
        contexto.Confirmacion.Respuesta = true;
        contexto.ViewModel.DesactivarEstudianteCommand.Execute(null);
        Assert.False(contexto.ViewModel.Estudiantes[0].EstaActivo);
        contexto.ViewModel.EstudianteSeleccionado = contexto.ViewModel.Estudiantes[0];
        contexto.ViewModel.ReactivarEstudianteCommand.Execute(null);
        Assert.True(contexto.ViewModel.Estudiantes[0].EstaActivo);
    }

    [Fact]
    public void FalloPersistenciaNoActualizaPantalla()
    {
        var contexto = ContextoPrueba.Cargado();
        contexto.ViewModel.AbrirCambioNombreCommand.Execute(null);
        contexto.ViewModel.NombreEdicionGrupo = "No confirmado";
        contexto.Gestion.ErrorRenombrarGrupo = new ErrorPersistenciaAplicacionException("detalle", new IOException("ruta secreta"));
        contexto.ViewModel.GuardarNombreGrupoCommand.Execute(null);
        Assert.Equal("Primero A", contexto.ViewModel.NombreGrupo);
        Assert.DoesNotContain("ruta secreta", contexto.Mensajes.Mensajes.Single(), StringComparison.Ordinal);
    }

    [Fact]
    public void EstaOcupadoBloqueaComandosYSeRestaura()
    {
        var contexto = new ContextoPrueba();
        contexto.Gestion.AlCrear = () => Assert.False(contexto.ViewModel.CrearGrupoCommand.CanExecute(null));
        contexto.ViewModel.NombreNuevoGrupo = "Primero A";
        contexto.ViewModel.CrearGrupoCommand.Execute(null);
        Assert.False(contexto.ViewModel.EstaOcupado);
        Assert.Equal(1, contexto.Gestion.Creaciones);
    }

    [Fact]
    public void ModeloVisualNoExponeIdentidades()
    {
        var nombres = typeof(EstudianteVisual).GetProperties().Select(x => x.Name).ToArray();
        Assert.DoesNotContain("Id", nombres);
        Assert.DoesNotContain("EstudianteId", nombres);
    }

    private sealed class ContextoPrueba
    {
        internal ContextoPrueba()
        {
            ViewModel = new(Gestion, Estado, Mensajes, Confirmacion);
        }

        internal GestionDoble Gestion { get; } = new();
        internal EstadoDoble Estado { get; } = new();
        internal MensajesDoble Mensajes { get; } = new();
        internal ConfirmacionDoble Confirmacion { get; } = new();
        internal GestionGrupoViewModel ViewModel { get; }

        internal static ContextoPrueba Cargado()
        {
            var contexto = new ContextoPrueba();
            contexto.Estado.Resultado = new(EstadoLecturaReferencia.Valida, contexto.Gestion.Grupo.GrupoId);
            contexto.ViewModel.Inicializar();
            return contexto;
        }
    }

    private sealed class EstadoDoble : IAlmacenamientoEstadoAplicacion
    {
        internal ResultadoLecturaReferencia Resultado { get; set; } = new(EstadoLecturaReferencia.Ausente);
        internal GrupoId? Guardado { get; private set; }
        internal bool Olvidada { get; private set; }
        public ResultadoLecturaReferencia Cargar() => Resultado;
        public void Guardar(GrupoId grupoId) => Guardado = grupoId;
        public void Olvidar() => Olvidada = true;
    }

    private sealed class MensajesDoble : IServicioMensajes
    {
        internal List<string> Mensajes { get; } = [];
        public void MostrarError(string mensaje) => Mensajes.Add(mensaje);
    }

    private sealed class ConfirmacionDoble : IServicioConfirmacion
    {
        internal bool Respuesta { get; set; } = true;
        public bool ConfirmarDesactivacion(string nombreEstudiante) => Respuesta;
    }

    private sealed class GestionDoble : IGestionGrupoPresentacion
    {
        private readonly GrupoId _grupoId = GrupoId.DesdeGuid(Guid.NewGuid());
        private readonly List<EstudianteDetalle> _estudiantes;

        internal GestionDoble()
        {
            _estudiantes =
            [
                new(EstudianteId.DesdeGuid(Guid.NewGuid()), "Ana", 1, true),
                new(EstudianteId.DesdeGuid(Guid.NewGuid()), "Beto", 2, false),
            ];
            Grupo = CrearDetalle("Primero A");
        }

        internal GrupoDetalle Grupo { get; private set; }
        internal Exception? ErrorCarga { get; set; }
        internal Exception? ErrorCrear { get; set; }
        internal Exception? ErrorAgregar { get; set; }
        internal Exception? ErrorRenombrarGrupo { get; set; }
        internal Exception? ErrorEditar { get; set; }
        internal Action? AlCrear { get; set; }
        internal int Creaciones { get; private set; }
        internal int Altas { get; private set; }
        internal int Desactivaciones { get; private set; }
        internal int Ediciones { get; private set; }
        internal int Renombrados { get; private set; }
        internal int CambiosNumero { get; private set; }

        public GrupoDetalle CrearGrupo(string nombreVisible)
        {
            Creaciones++;
            AlCrear?.Invoke();
            Lanzar(ErrorCrear);
            Grupo = CrearDetalle(nombreVisible);
            return Grupo;
        }

        public GrupoDetalle CargarGrupo(GrupoId grupoId)
        {
            Lanzar(ErrorCarga);
            return Grupo;
        }

        public IReadOnlyList<GrupoDetalle> ListarGrupos()
        {
            return [Grupo];
        }

        public GrupoDetalle CambiarNombreGrupo(GrupoId grupoId, string nombreVisible)
        {
            Lanzar(ErrorRenombrarGrupo);
            Grupo = CrearDetalle(nombreVisible);
            return Grupo;
        }

        public EstudianteDetalle AgregarEstudiante(
            GrupoId grupoId,
            string nombreVisible,
            int numeroLista,
            string primerApellido = "",
            string segundoApellido = "",
            string nombres = "",
            DateOnly? fechaNacimiento = null,
            GeneroEstudiante genero = GeneroEstudiante.NoEspecificado,
            DateOnly? fechaIngreso = null,
            string observaciones = "")
        {
            Altas++;
            Lanzar(ErrorAgregar);
            var estudiante = new EstudianteDetalle(
                EstudianteId: EstudianteId.DesdeGuid(Guid.NewGuid()),
                NombreVisible: nombreVisible,
                PrimerApellido: primerApellido,
                SegundoApellido: segundoApellido,
                Nombres: nombres,
                FechaNacimiento: fechaNacimiento,
                Edad: null,
                Genero: genero,
                FechaIngreso: fechaIngreso,
                Observaciones: observaciones,
                NumeroLista: numeroLista,
                EstaActivo: true);
            _estudiantes.Add(estudiante);
            Grupo = CrearDetalle(Grupo.NombreVisible);
            return estudiante;
        }

        public EstudianteDetalle RenombrarEstudiante(GrupoId grupoId, EstudianteId id, string nombre)
        {
            Renombrados++;
            var actual = Buscar(id);
            Reemplazar(actual with { NombreVisible = nombre });
            return Buscar(id);
        }

        public EstudianteDetalle CambiarNumeroLista(GrupoId grupoId, EstudianteId id, int numero)
        {
            CambiosNumero++;
            var actual = Buscar(id);
            Reemplazar(actual with { NumeroLista = numero });
            return Buscar(id);
        }

        public EstudianteDetalle EditarEstudiante(
            GrupoId grupoId,
            EstudianteId id,
            string nombre,
            int numero,
            string primerApellido = "",
            string segundoApellido = "",
            string nombres = "",
            DateOnly? fechaNacimiento = null,
            GeneroEstudiante genero = GeneroEstudiante.NoEspecificado,
            DateOnly? fechaIngreso = null,
            string observaciones = "")
        {
            Ediciones++;
            Lanzar(ErrorEditar);
            var actual = Buscar(id);
            Reemplazar(actual with { NombreVisible = nombre, NumeroLista = numero, PrimerApellido = primerApellido, SegundoApellido = segundoApellido, Nombres = nombres, FechaNacimiento = fechaNacimiento, Genero = genero, FechaIngreso = fechaIngreso, Observaciones = observaciones });
            return Buscar(id);
        }

        public EstudianteDetalle DesactivarEstudiante(GrupoId grupoId, EstudianteId id)
        {
            Desactivaciones++;
            var actual = Buscar(id);
            Reemplazar(actual with { EstaActivo = false });
            return Buscar(id);
        }

        public EstudianteDetalle ReactivarEstudiante(GrupoId grupoId, EstudianteId id)
        {
            var actual = Buscar(id);
            Reemplazar(actual with { EstaActivo = true });
            return Buscar(id);
        }

        public IReadOnlyList<EstudianteDetalle> ObtenerTodosLosEstudiantes(GrupoId grupoId) => Grupo.Estudiantes;

        private GrupoDetalle CrearDetalle(string nombre) => new(_grupoId, nombre, _estudiantes.ToArray());
        private EstudianteDetalle Buscar(EstudianteId id) => _estudiantes.Single(x => x.EstudianteId == id);
        private void Reemplazar(EstudianteDetalle estudiante)
        {
            _estudiantes[_estudiantes.FindIndex(x => x.EstudianteId == estudiante.EstudianteId)] = estudiante;
            Grupo = CrearDetalle(Grupo.NombreVisible);
        }
        private static void Lanzar(Exception? exception)
        {
            if (exception is not null) throw exception;
        }
    }
}