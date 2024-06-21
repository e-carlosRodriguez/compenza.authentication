
namespace compenza.authentication.domain.Entities
{
    public class PerfilProceso
    {
        public int cvePerfil { get; set; }
        public int cveProceso { get; set; }
        public bool bEscribir { get; set; }
        public bool bEliminar { get; set; }
        public bool bEjecutar { get; set; }
        public bool bAutorizar { get; set; }
        public bool bExcel { get; set; }
        public bool cveSistema { get; set; }
    }
}
