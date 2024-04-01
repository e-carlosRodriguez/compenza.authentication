
using compenza.authentication.domain.Entities;

namespace compenza.authentication.application.Utilities
{
    public interface ITokenProvider
    {
        string GetTokenAsync(Empleado empleado, int cveSistemas, string nombreCliente, IEnumerable<Permisos> permisos);
    }
}
