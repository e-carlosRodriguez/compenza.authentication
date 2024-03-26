
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using compenza.authentication.domain.Configure;
using compenza.authentication.percistance.Interfaces;

namespace compenza.authentication.percistance
{
    public class CompenzaDbContext : ICompenzaDbContext
    {
        private readonly Settings _settings;

        public CompenzaDbContext(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        public SqlConnection GetConeConnection()
        {
            return new SqlConnection( _settings.conexion.AXO );
        }
    }
}
