using compenza.authentication.domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace compenza.authentication.percistance.Interfaces
{
    public interface ILoginRepository
    {
        Task<Empleado?> ConsultarLogin(string loginUser);
        Task<Configuracion> ObtenerConfiguracion(int configuracionId);
        Task<int> CargarPersimosUsuarioProverdor(int cveUsuario);
        Task<IEnumerable<Permisos>> ListarPermisos(int proceso, int cveUsuario);
        Task<IEnumerable<int>> TieneFamilias(int periocidad, int cveEmpleado, bool bConfirmado);
        Task<int> TieneMensajes(int cveEmpleado, DateTime fechaVijencia);
        Task AuditoriaProcesos(int cveProceso, int cveAccion, int cveUsuario);
        Task<(int, int)> UsuarioEmpleadoActivos();
        Task<(int, int)> UsuarioEmpleadoActivosTotal();
        Task<DataTable> BaseDeDatosId();
    }
}
