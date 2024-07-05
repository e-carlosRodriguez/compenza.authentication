using compenza.authentication.domain.Entities;

namespace compenza.authentication.percistance.Interfaces
{
    public interface IProcesosRepository
    {
        Task<PerfilProceso> CargarProceso(int CveProceso, int CvePerfil, int CveUsuario);
        Task Auditoria(int CveProceso, int accion, int CveUsuario);
    }
}
