using compenza.authentication.domain.Entities;

namespace compenza.authentication.percistance.Interfaces
{
    public interface ILoginRepository
    {
        Task<Empleado?> ConsultarLogin(string loginUser);
        Task<Configuracion> ObtenerConfiguracion( int configuracionId );
        Task<int> CargarPersimosUsuarioProverdor( int cveUsuario );
        Task<IEnumerable<Permisos>?> ListarPermisos(int proceso, int cveUsuario);
    }
}
