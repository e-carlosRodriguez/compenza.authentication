using MediatR;
using compenza.authentication.domain.Enums;
using compenza.authentication.domain.Configure;
using compenza.authentication.percistance.Interfaces;
using compenza.authentication.application.Utilities;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace compenza.authentication.application.Querys
{
    public class ValidateLicenseBeforeUploading
    {
        public record Query(HttpContext Context) : IRequest<Result>;

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly ILoginRepository _loginRepository;

            public Handler(ILoginRepository loginRepository)
            {
                _loginRepository = loginRepository;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var (Empleados, usuarios) = await _loginRepository.UsuarioEmpleadoActivos();
                var (EmpleadosTotal, UsuariosTotal) = await _loginRepository.UsuarioEmpleadoActivosTotal();

                PropertiesConfig propertiesConfig = new PropertiesConfig();

                Parameters LicenseResult = await propertiesConfig.Information();

                string strPath = request.Context.Request.Host.Value + "/";
                string ServerIDConfiguration = string.Empty;

                if (!string.IsNullOrEmpty(strPath))
                {
                    ServerIDConfiguration = Encripta.EncriptarCadena(strPath);
                }

                var dtName = await _loginRepository.BaseDeDatosId();

                var CPU = dtName.Rows[0]["ComputerName"];
                var server = dtName.Rows[0]["InstanceName"];
                var version = dtName.Rows[0]["ProductVersion"];

                var BDServerIDConfiguration = Encripta.EncriptarCadena(String.Format("{0}\\{1}\\{2}", CPU, server, version));

                int validarServer = await propertiesConfig.ValidarServer(BDServerIDConfiguration, ServerIDConfiguration);
                var isVlidResult = await ValidarLicencia(LicenseResult, Empleados, usuarios, EmpleadosTotal, UsuariosTotal, validarServer);

                return isVlidResult;
            }

            public async Task<Result> ValidarLicencia(Parameters license, int empleados, int usuarios, int empleadosTotal, int usuarioTotal, int validarServer)
            {
                PropertiesConfig _propertiesConfig = new PropertiesConfig();

                var result = new Result();
                var diasTolerancia = license.FechaLicencia.Subtract(DateTime.Now).Days;
                var diasExpiracion = DateTime.Now.Subtract(license.FechaLicencia).Days;
                result.Actualizarlicencia = false;

                if (DateTime.Now >= license.FechaLicencia && diasExpiracion <= license.Expira)
                {
                    result.Actualizarlicencia = true;
                }

                if (license is null)
                {
                    result.Mensaje = "msgLicenciaInvalida";
                    result.Objeto = (int)eResultado.ErrorLicencia;
                    result.Res = false;

                    return result;
                }
                else if (validarServer == (int)eTipoErrors.ErrorArchivo)
                {
                    result.Mensaje = "msgServidorInvalido";
                    result.Objeto = (int)eTipoErrors.ServidorInvalido;
                    result.DBServerMessage = license.BDServerID == "" ? null : $"ID: {license.BDServerID}";
                    result.ServerMessage = license.BDServerID == "" ? null : $"ID: {license.ServerID}";
                    result.Res = false;

                    return result;
                }
                else if (diasTolerancia <= license.Alerta && diasTolerancia is not < 0)
                {
                    result.Mensaje = diasTolerancia.ToString();
                    result.Objeto = (int)eTipoErrors.LicenciasAlertas;
                    result.Res = true;

                    return result;
                }
                else if (DateTime.Now >= license.FechaLicencia && diasExpiracion <= license.Expira)
                {
                    result.Mensaje = "msgDiasDeGracia";
                    result.Res = true;
                    result.Objeto = (int)eTipoErrors.LicenciaUltimoDiaconLicencia;
                    return result;
                }
                else if (DateTime.Now >= license.FechaLicencia.AddDays(license.Expira))
                {
                    result.Mensaje = "msgLicenciaExpirada";
                    result.Res = false;
                    result.Objeto = (int)eTipoErrors.LicenciaVencida;
                    return result;
                }
                else if (empleados > (license.Empleados + (license.Empleados * 0.3)))
                {
                    result.Mensaje = "msgLicenciaLimiteEmpleados";
                    result.Res = false;
                    result.Objeto = (int)eTipoErrors.ToleranciaEmpleados;
                    return result;
                }
                else if (usuarios > (license.Usuarios + (license.Usuarios * 0.3)))
                {
                    result.Mensaje = "msgLicenciaLimiteUsuarios";
                    result.Res = false;
                    result.Objeto = (int)eTipoErrors.ToleranciaUsuarios;
                    return result;
                }
                else if (await _propertiesConfig.ValidarEmpleados(empleadosTotal) == (int)eTipoErrors.ExeceEmpleados)
                {
                    result.Mensaje = "msgLicenciaLimiteEmpleados";
                    result.Res = false;
                    result.Objeto = (int)eTipoErrors.ToleranciaEmpleados;
                    return result;
                }
                else if (await _propertiesConfig.ValidarEmpleados(empleadosTotal) == (int)eTipoErrors.ExeceUsuarios)
                {
                    result.Mensaje = "msgLicenciaLimiteUsuarios";
                    result.Res = false;
                    result.Objeto = (int)eTipoErrors.ToleranciaUsuarios;
                    return result;
                }

                return result;
            }
        }
    }

    public class ObtenerConfiguracionesPorId
    {
        public record Query(int Accion, int configuracionFotos) : IRequest<DataSet>;

        public class Handler : IRequestHandler<Query, DataSet>
        {

            private readonly ILoginRepository _iLoginRepository;

            public Handler(ILoginRepository iLoginRepository)
            {
                _iLoginRepository = iLoginRepository ?? throw new ArgumentException(nameof(iLoginRepository));
            }

            public async Task<DataSet> Handle(Query query, CancellationToken cancellationToken)
            {
                var Path = await _iLoginRepository.ObtenerConfiguracionesPorId(query.Accion, query.configuracionFotos);

                return Path;
            }
        }
    }
}
