using compenza.authentication.domain.Entities;

namespace compenza.authentication.percistance.Interfaces
{
    public interface ILoginRepository
    {
        Task<Empleado?> ConsultarLogin(string loginUser);
    }
}
