using License;
using MediatR;
using compenza.authentication.domain.Enums;
using compenza.authentication.domain.Configure;
using compenza.authentication.percistance.Interfaces;
using compenza.authentication.application.Utilities;
using Microsoft.AspNetCore.Http;

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
                var ( Empleados, usuarios ) = await _loginRepository.UsuarioEmpleadoActivos();
                var ( EmpleadosTotal, UsuariosTotal) = await _loginRepository.UsuarioEmpleadoActivosTotal();

                var LicenseResult = License.PropertiesConfig.Information();

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

                int validarServer = PropertiesConfig.ValidarServer(BDServerIDConfiguration, ServerIDConfiguration);
                var isVlidResult = ValidarLicencia( LicenseResult, Empleados, usuarios, EmpleadosTotal, UsuariosTotal, validarServer);

                return isVlidResult;
            }

            public Result ValidarLicencia(License.Parameters license, int empleados, int usuarios, int empleadosTotal, int usuarioTotal, int validarServer)
            {

                PropertiesConfig _propertiesConfig = new PropertiesConfig();
                var result = new Result();
                var diasTolerancia = license.FechaLicencia.Subtract(DateTime.Now).Days;

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
                    result.Objeto = (int)eTipoErrors.ErrorArchivo;
                    result.DBServerMessage = $"ID: {license.BDServerID}";
                    result.ServerMessage = $"ID: {license.ServerID}";
                    result.Res = false;


                    return result;
                }
                else if (diasTolerancia <= license.Alerta && diasTolerancia is not < 0)
                {
                    result.Mensaje = "msgAlertaLicencia";
                    result.Objeto = diasTolerancia;
                    result.Res = false;

                    return result;
                }
                else if(DateTime.Now >= license.FechaLicencia)
                {
                    result.Mensaje = "msgAlertaLicenciaCaducado";
                    result.Res = false;
                }
                else if (DateTime.Now >= license.FechaLicencia.AddDays(license.Expira))
                {
                    result.Mensaje = "msgLicenciaExpirada";
                    result.Res = false;

                    return result;
                }
                else if (empleados > (license.Empleados + (license.Empleados * 0.3)))
                {
                    result.Mensaje = "msgLicenciaLimiteEmpleados";
                    result.Res = false;

                    return result;
                }
                else if (usuarios > (license.Usuarios + (license.Usuarios * 0.3)))
                {
                    result.Mensaje = "msgLicenciaLimiteUsuarios";
                    result.Res = false;

                    return result;
                }
                else if (_propertiesConfig.ValidarEmpleados(empleadosTotal) == (int)eTipoErrors.ExeceEmpleados)
                {
                    result.Mensaje = "msgLicenciaLimiteEmpleados";
                    result.Res = false;

                    return result;
                }
                else if (_propertiesConfig.ValidarEmpleados(empleadosTotal) == (int)eTipoErrors.ExeceUsuarios)
                {
                    result.Mensaje = "msgLicenciaLimiteUsuarios";
                    result.Res = false;

                    return result;
                }

                return result;
            }

        }
    }
}
