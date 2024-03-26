using Dapper;
using Dapper.Contrib;
using compenza.authentication.domain.Entities;
using compenza.authentication.domain.Parameters;
using compenza.authentication.percistance.Interfaces;
using Dapper.Contrib.Extensions;

namespace compenza.authentication.percistance.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly ICompenzaDbContext _compenzaDbContext;

        public LoginRepository(ICompenzaDbContext compenzaDbContext)
        {
            _compenzaDbContext = compenzaDbContext;
        }

        public async Task<IEnumerable<Permisos>?> ListarPermisos(int proceso, int cveUsuario)
        {
            using (var conn = _compenzaDbContext.GetConeConnection())
            {
                var result = await conn.QueryAsync<Permisos>("Compenza.uSP_Compenza_GetMenuPrincipal", new { 
                    Accion = 3,
                    Proceso = proceso,
                    Perfil = cveUsuario
                }, commandType: System.Data.CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<int> CargarPersimosUsuarioProverdor(int cveUsuario)
        {
            using (var conn = _compenzaDbContext.GetConeConnection())
            {
                var dimenciones = await conn.QueryAsync<int>("Viajes.SP_UsuariosDimenciones", new { 
                    cveUsuario = cveUsuario, cveDimencion = 3 }, commandType: System.Data.CommandType.StoredProcedure);

                return dimenciones.FirstOrDefault();
            }
        }

        public async Task<Empleado?> ConsultarLogin(string loginUser)
        {
            using ( var con = _compenzaDbContext.GetConeConnection() )
            {
                var esValido = await con.QueryAsync<Empleado>("Portal.SP_Portal_Empleados", new CrearEmpleados(Accion: 1, loginUser: loginUser), commandType: System.Data.CommandType.StoredProcedure);
                return esValido.FirstOrDefault();
            }
        }

        public async Task<Configuracion> ObtenerConfiguracion(int configuracionId)
        {
            using (var con = _compenzaDbContext.GetConeConnection())
            {
                var configuracion = await con.GetAsync<Configuracion>(configuracionId);
                return configuracion;
            }
        }
    

    }
}
