using compenza.authentication.domain.Entities;
using compenza.authentication.percistance.Interfaces;
using Dapper;
using System.Data;

namespace compenza.authentication.api.Controllers
{
    public class PerfilProcesoRepository : IProcesosRepository
    {

        private readonly ICompenzaDbContext _dbContext;

        public PerfilProcesoRepository(ICompenzaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Auditoria(int CveProceso, int accion, int CveUsuario)
        {
            using (var conn = _dbContext.GetConeConnection())
            {
                var reuslt = await conn.ExecuteAsync("Compenza.SP_Compenza_BitacoraSistema", new {

                    Accion = 1,
                    cveProceso = CveProceso,
                    cveAccion = accion,
                    cveUsuario = CveUsuario,
                    fechaInicial = DateTime.Now,
                    fechaFinal = DateTime.Now

                }, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<PerfilProceso?> CargarProceso(int CveProceso, int CvePerfil, int CveUsuario)
        {
            using (var con = _dbContext.GetConeConnection())
            {
                var result = await con.QueryAsync<PerfilProceso>("Compenza.uSP_Compenza_GetMenuPrincipal", new { Accion = 2, Proceso = CveProceso, Perfil = CvePerfil }, commandType: CommandType.StoredProcedure);
                return result.FirstOrDefault();
            }
        }
    }
}
