using MediatR;
using Microsoft.Extensions.Options;
using compenza.authentication.domain.Enums;
using compenza.authentication.domain.Entities;
using compenza.authentication.domain.Configure;
using compenza.authentication.application.Utilities;
using compenza.authentication.percistance.Interfaces;
using compenza.authentication.application.Exceptions;

namespace compenza.authentication.application.Querys
{
    public class AuthenticateUser
    {
        public record Query(
            string LoginName, 
            string Password, 
            string Dominio, 
            string language, 
            bool bEncriptado = false
        ) :IRequest<Result>;

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly ILoginRepository _loginRepository;
            private readonly IStringsEncrypter _encrypter;
            private readonly ITokenProvider _tokenProvider;
            private readonly Settings _settings;

            public Handler(ILoginRepository loginRepository, IOptions<Settings> Settings, IStringsEncrypter encrypter, ITokenProvider tokenProvider)
            {
                _loginRepository = loginRepository;
                _settings = Settings.Value;
                _encrypter = encrypter;
                _tokenProvider = tokenProvider;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = new Result();

                var dsEmpleado = await _loginRepository.ConsultarLogin(request.LoginName);
                var cveEmpleado = Convert.ToInt32(dsEmpleado?.cveEmpleado) > 1;

                if (dsEmpleado is null || (!dsEmpleado.bActivo && (!dsEmpleado.bIngresaPortal || cveEmpleado)) || (!_settings.Debug && !(dsEmpleado.password == _encrypter.EncriptarCadena(request.Password))))
                {
                    result.Mensaje =
                       dsEmpleado is null || !(!_settings.Debug && dsEmpleado.password == _encrypter.EncriptarCadena(request.Password)) ? "lblUsuarioIncorrecto" :
                       !dsEmpleado.bActivo ? "lblUsuarioInactivo" :
                       (!dsEmpleado.bActivo && (!dsEmpleado.bIngresaPortal || cveEmpleado)) ? "lblUsuarioSinPortal" : "Contactar a soporte";
                    result.Res = false;
                    result.Objeto = (int)eResultado.Error;

                    throw new HttpException(System.Net.HttpStatusCode.BadRequest, "Bad request", result);
                }
                if (dsEmpleado.cveUsuario > 0)
                {
                    var cambiarPassword = DateTime.Parse(dsEmpleado.cambiarPassword);
                    var diasCambio = await _loginRepository.ObtenerConfiguracion((int)eConfiguracionSistema.diasCambioPassword);

                    if ((dsEmpleado.UsExterno is not null && (bool)dsEmpleado.UsExterno) &&
                        DateTime.Compare(cambiarPassword.AddDays(diasCambio.ValorEntero), DateTime.Today) < 0)
                    {
                        result.Objeto = (int)eResultado.CambioContra;
                        result.Mensaje = dsEmpleado.cveUsuario.ToString();
                        return result;
                    }
                }

                var cvePerfil = Convert.ToInt32(dsEmpleado.cvePerfil);
                var permisos = await _loginRepository.ListarPermisos(0, cvePerfil);
                var proceso = ProcesoPortal(permisos);

                //se tomara en cuenta solo Portal por el momento
                var claveSistema =
                    proceso.Portal ? (int)eSistemasCompenza.Portal :
                    proceso.Objetivos ? (int)eSistemasCompenza.Objetivos :
                    proceso.Viajes ? (int)eSistemasCompenza.Viajes : 2;

                if (claveSistema is 2)
                {
                    var nombreCliente = await _loginRepository.ObtenerConfiguracion((int)eConfiguracionSistema.NombreCliente);
                    var TieneMesajesResult = await TieneMensajes(dsEmpleado.cveEmpleado);
                    var ValidarFamiliasRevision = _loginRepository.TieneFamilias(0, Convert.ToInt32(dsEmpleado.cveEmpleado), true);
                    var UsuarioEmpleadoActivos = _loginRepository.UsuarioEmpleadoActivos();
                    var procesoInicio = permisos.First(item => item.cveProceso == (int)eProcesosMenu.PortalInicio);

                    if (cvePerfil <= 0 || (procesoInicio is not null && procesoInicio.bAutorizar))
                    {

                        await Task.WhenAll(new Task[] { ValidarFamiliasRevision, UsuarioEmpleadoActivos });
                        var ValidarFamiliasRevisionResult = ValidarFamiliasRevision.IsCompletedSuccessfully ? ValidarFamiliasRevision.Result.Count() : 0;
                        var ( empleados, usuarios ) = UsuarioEmpleadoActivos.IsCompletedSuccessfully ? UsuarioEmpleadoActivos.Result : (0,0);


                        var licencia = License.PropertiesConfig.Information();                       
                        var tienReglas = ValidarLicencia(licencia, empleados, usuarios, out result);

                        result.Objeto = (int)eResultado.Redirect;
                        result.Mensaje = (tienReglas && (ValidarFamiliasRevisionResult > 0)) ? "Revision/Revision" : "/";

                        if (TieneMesajesResult is not null && TieneMesajesResult.Res)
                        {
                            result.Objeto = TieneMesajesResult.Objeto;
                            result.Mensaje = TieneMesajesResult.Mensaje;
                        }

                        if (!tienReglas && !TieneMesajesResult.Res)
                        {
                            result.Mensaje = "/";
                        }

                        await _loginRepository.AuditoriaProcesos((int)eProcesosMenu.PortalAclaraciones, (int)eAcciones.Login, dsEmpleado.cveUsuario);
                        var token = _tokenProvider.GetTokenAsync(dsEmpleado, claveSistema, nombreCliente.ValorString, permisos /*reglas de licencia*/);
                        result.Token = token;

                    }
                    else
                    {
                        var procesoAcalaraciones = permisos.First(item => item.cveProceso == (int)eProcesosMenu.PortalAclaraciones);
                        if ((procesoAcalaraciones is not null && procesoAcalaraciones.bAutorizar))
                        {
                            if (TieneMesajesResult.Res)
                            {
                                result.Objeto = TieneMesajesResult.Objeto;
                                result.Mensaje = TieneMesajesResult.Mensaje;
                            }
                            else
                            {
                                result.Objeto = (int)eResultado.Redirect;
                                result.Mensaje = "/Tickets/Tickets";
                            }

                            await _loginRepository.AuditoriaProcesos((int)eProcesosMenu.PortalAclaraciones, (int)eAcciones.Login, dsEmpleado.cveUsuario);
                            var token = _tokenProvider.GetTokenAsync(dsEmpleado, claveSistema, nombreCliente.ValorString, permisos /*reglas de licencia*/);
                            result.Token = token;
                        }
                    }            
                }

                return result;
            }

            private PermisoProceso ProcesoPortal(IEnumerable<Permisos> perfilProceso)
            {
                bool permisoPortal = perfilProceso.AsEnumerable().Any(x =>
                   x.cveProceso == (int)eProcesosMenu.PortalInicio
                   || x.cveProceso == (int)eProcesosMenu.PortalAclaraciones);

                bool permisoObjetivos = perfilProceso.AsEnumerable().Any(x =>
                    x.cveProceso == (int)eProcesosMenu.InicioObjetivos);

                bool permisoViajes = perfilProceso.AsEnumerable().Any(x =>
                    x.cveProceso == (int)eProcesosMenu.InicioViajes);

                return new PermisoProceso()
                {
                    Portal = permisoPortal,
                    Objetivos = permisoObjetivos,
                    Viajes = permisoViajes
                };
            }

            private async Task<Result> TieneMensajes( int cveEmpleado )
            {
                var response = new Result();
                var result = await _loginRepository.TieneMensajes(cveEmpleado, DateTime.Now);
                if (result > 0)
                {
                    response.Objeto = (int)eResultado.Redirect;
                    response.Mensaje = "Mensajes/Mensajes";

                    return response;
                }

                response.Res = false;
                return response;
            }
            
            private bool ValidarLicencia(License.Parameters license, int empleados, int usuarios, out Result result)
            {
                var isValidLisence = true;
                result = new Result();

                if (license is null)
                {
                    isValidLisence = false;
                    result.Mensaje = "msgLicenciaInvalida";
                    result.Objeto = isValidLisence ? "" : (int)eResultado.ErrorLicencia;
                }
                if (DateTime.Now >= license.FechaLicencia.AddDays(license.Expira))
                {
                    result.Mensaje = "msgLicenciaExpirada";
                    isValidLisence = false;
                }
                if (empleados > (license.Empleados + (license.Empleados * 0.3)))
                {
                    result.Mensaje = "msgLicenciaLimiteEmpleados";
                    isValidLisence = false;
                }
                if (usuarios > (license.Usuarios + (license.Usuarios * 0.3)))
                {
                    result.Mensaje = "msgLicenciaLimiteUsuarios";
                    isValidLisence = false;
                }

                return isValidLisence;
            }
        }
    }
}
