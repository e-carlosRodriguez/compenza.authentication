
using System.Data.SqlClient;

namespace compenza.authentication.percistance.Interfaces
{
    public interface ICompenzaDbContext
    {
        SqlConnection GetConeConnection();
    }
}
