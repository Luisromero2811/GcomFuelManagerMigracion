namespace GComFuelManager.Shared.Modelos
{
    public class CRMActividadDocumento
    {
        public int ActividadId { get; set; }
        public int DocumentoId { get; set; }

        public CRMActividades? Actividad { get; set; } = null!;
        public CRMDocumento? Documento { get; set; } = null!;
    }
}
