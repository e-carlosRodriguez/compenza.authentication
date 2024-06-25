using compenza.authentication.application.Utilities;
using compenza.authentication.domain.Configure;
using compenza.authentication.domain.Entities;
using compenza.authentication.domain.Enums;
using compenza.authentication.percistance.Interfaces;
using License;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Data;

namespace compenza.authentication.application.Querys
{
    public class AuthenticateUser
    {
        public record Query(
            string LoginName,
            string Password,
            string Dominio,
            string language,
            HttpContext Context,
            bool bEncriptado = false
        ) : IRequest<Result>;

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly ILoginRepository _loginRepository;
            private readonly IStringsEncrypter _encrypter;
            private readonly ITokenProvider _tokenProvider;
            private readonly Settings _settings;

            public Handler(ILoginRepository loginRepository, IOptions<Settings> Settings, IStringsEncrypter encrypter,
                ITokenProvider tokenProvider)
            {
                _loginRepository = loginRepository;
                _settings = Settings.Value;
                _encrypter = encrypter;
                _tokenProvider = tokenProvider;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = new Result();

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

                if (validarServer == (int)eTipoErrors.ErrorArchivo)
                {
                    result.Mensaje = "msgServidorInvalido";
                    result.Objeto = (int)eTipoErrors.ErrorArchivo;

                    return result;
                }

                var dsEmpleado = await _loginRepository.ConsultarLogin(request.LoginName);

                if (!IsValidEmpleado(dsEmpleado, request.Password))
                {
                    return HandleInvalidEmpleado(dsEmpleado, result);
                }

                var cveUsuario = Convert.ToInt32(dsEmpleado?.cveUsuario) > 0;

                if (cveUsuario && await IsPasswordChangeRequired(dsEmpleado))
                {
                    return HandlePasswordChangeRequired(dsEmpleado, result);
                }

                var cvePerfil = Convert.ToInt32(dsEmpleado.cvePerfil);
                var permisos = await _loginRepository.ListarPermisos(0, cvePerfil);
                var proceso = ProcesoPortal(permisos);

                var perfilProceso = new DataTable();

                perfilProceso = await _loginRepository.PermisoPerfil(3, 0, cvePerfil);

                var permisosPortal = PermisoPortal(perfilProceso) || cvePerfil == 0;

                if (!permisosPortal)
                {
                    result.Mensaje = "msgSinPermisoaPortal";
                    result.Objeto = (int)eTipoErrors.ModuloNoEncontrado;

                    return result;
                }

                // Lógica para determinar el sistema clave
                var claveSistema = DetermineClaveSistema(proceso);

                if (claveSistema == 2)
                {
                    var nombreCliente = await _loginRepository.ObtenerConfiguracion((int)eConfiguracionSistema.NombreCliente);

                    if (await CanAccessPortal(dsEmpleado, permisos))
                    {
                        return await HandlePortalAccess(dsEmpleado, claveSistema, nombreCliente, permisos, result);
                    }
                    else
                    {
                        return await HandleAclaracionesAccess(dsEmpleado, claveSistema, nombreCliente, permisos);
                    }
                }

                return result;
            }

            private bool IsValidEmpleado(Empleado dsEmpleado, string password)
            {
                return dsEmpleado != null &&
                       (dsEmpleado.bActivo || dsEmpleado.bIngresaPortal || Convert.ToInt32(dsEmpleado?.cveEmpleado) > 1) &&
                       (_settings.Debug || dsEmpleado.password == _encrypter.EncriptarCadena(password));
            }

            private Result HandleInvalidEmpleado(Empleado dsEmpleado, Result result)
            {
                result.Res = false;
                result.Objeto = (int)eResultado.Error;

                if (dsEmpleado is null)
                {
                    result.Mensaje = "lblUsuarioIncorrecto";
                }
                else if (!dsEmpleado.bActivo)
                {
                    result.Mensaje = "lblUsuarioInactivo";
                }
                else if (!dsEmpleado.bIngresaPortal || Convert.ToInt32(dsEmpleado?.cveEmpleado) > 1)
                {
                    result.Mensaje = "lblUsuarioSinPortal";
                }
                else
                {
                    result.Mensaje = "Contactar a soporte";
                }

                return result;
            }

            private async Task<bool> IsPasswordChangeRequired(Empleado dsEmpleado)
            {
                if (dsEmpleado.UsExterno is not null && (bool)dsEmpleado.UsExterno)
                {
                    var cambiarPassword = DateTime.Parse(dsEmpleado.cambiarPassword);
                    var diasCambio = await _loginRepository.ObtenerConfiguracion((int)eConfiguracionSistema.diasCambioPassword);

                    return DateTime.Compare(cambiarPassword.AddDays(diasCambio.ValorEntero), DateTime.Today) < 0;
                }

                return false;
            }

            private Result HandlePasswordChangeRequired(Empleado dsEmpleado, Result result)
            {
                result.Objeto = (int)eResultado.CambioContra;
                result.Mensaje = dsEmpleado.cveUsuario.ToString();
                result.Mensaje = "Cambio de contraseña requerido.";
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

            private int DetermineClaveSistema(PermisoProceso proceso)
            {
                int claveSistema =
                    proceso.Portal ? (int)eSistemasCompenza.Portal :
                    proceso.Objetivos ? (int)eSistemasCompenza.Objetivos :
                    proceso.Viajes ? (int)eSistemasCompenza.Viajes : 2;

                return claveSistema;
            }

            private async Task<bool> CanAccessPortal(Empleado dsEmpleado, IEnumerable<Permisos> permisos)
            {
                var cvePerfil = Convert.ToInt32(dsEmpleado.cvePerfil);
                var procesoInicio = permisos.FirstOrDefault(item => item.cveProceso == (int)eProcesosMenu.PortalInicio);

                return cvePerfil <= 0 || (procesoInicio != null && procesoInicio.bAutorizar);
            }

            private async Task<Result> HandlePortalAccess(Empleado dsEmpleado, int claveSistema, Configuracion nombreCliente, IEnumerable<Permisos> permisos, Result result)
            {
                var TieneMesajesResult = await TieneMensajes(dsEmpleado.cveEmpleado);
                var ValidarFamiliasRevision = await _loginRepository.TieneFamilias(0, Convert.ToInt32(dsEmpleado.cveEmpleado), true);
                var UsuarioEmpleadoActivos = await _loginRepository.UsuarioEmpleadoActivos();
                var UsuarioEmpleadoActivosTotal = await _loginRepository.UsuarioEmpleadoActivosTotal();
                var procesoInicio = permisos.First(item => item.cveProceso == (int)eProcesosMenu.PortalInicio);

                var (empleados, usuarios) = UsuarioEmpleadoActivos;
                var (empleadosTotal, usuariosTotal) = UsuarioEmpleadoActivosTotal;

                var licencia = License.PropertiesConfig.Information();
                var tienReglas = ValidarLicencia(licencia, empleados, usuarios, empleadosTotal, usuariosTotal, out result);

                result.cvUsuario = dsEmpleado.cveUsuario;
                result.cvEmpleado = dsEmpleado.cveEmpleado;
                result.AdministraPortal = dsEmpleado.bAdministraPortal;
                result.Objeto = (int)eResultado.Redirect;
                result.Mensaje = (tienReglas && ((int)ValidarFamiliasRevision.FirstOrDefault() > 0)) ? "Revision/Revision" : "/home";

                if (TieneMesajesResult is not null && TieneMesajesResult.Res)
                {
                    result.Objeto = TieneMesajesResult.Objeto;
                    result.Mensaje = TieneMesajesResult.Mensaje;
                }

                if (!tienReglas && !TieneMesajesResult.Res)
                {
                    result.Mensaje = "/home";
                }

                await _loginRepository.AuditoriaProcesos((int)eProcesosMenu.PortalAclaraciones, (int)eAcciones.Login, dsEmpleado.cveUsuario);
                var token = _tokenProvider.GetTokenAsync(dsEmpleado, claveSistema, nombreCliente.ValorString, permisos /*reglas de licencia*/);
                result.sNombre = dsEmpleado.Nombre;
                result.Token = token;

                return result;
            }

            private async Task<Result> TieneMensajes(int cveEmpleado)
            {
                var response = new Result();

                var result = await _loginRepository.TieneMensajes(cveEmpleado, DateTime.Now);

                if (result > 0)
                {
                    response.Objeto = (int)eResultado.Redirect;
                    response.Mensaje = "/home/mensajes/administrar";

                    return response;
                }

                response.Res = false;

                return response;
            }

            public bool ValidarLicencia(License.Parameters license, int empleados, int usuarios, int empleadosTotal, int usuarioTotal, out Result result)
            {
                var isValidLicence = true;

                PropertiesConfig _propertiesConfig = new PropertiesConfig();

                result = new Result();

                if (license is null)
                {
                    isValidLicence = false;
                    result.Mensaje = "msgLicenciaInvalida";
                    result.Objeto = isValidLicence ? "" : (int)eResultado.ErrorLicencia;
                }
                else if (((int)(DateTime.Now - license.FechaLicencia).TotalDays) == 5)
                {
                    result.Mensaje = "msgAlertaLicencia";
                    isValidLicence = false;
                }
                else if (DateTime.Now >= license.FechaLicencia.AddDays(license.Expira))
                {
                    result.Mensaje = "msgLicenciaExpirada";
                    isValidLicence = false;
                }
                else if (empleados > (license.Empleados + (license.Empleados * 0.3)))
                {
                    result.Mensaje = "msgLicenciaLimiteEmpleados";
                    isValidLicence = false;
                }
                else if (usuarios > (license.Usuarios + (license.Usuarios * 0.3)))
                {
                    result.Mensaje = "msgLicenciaLimiteUsuarios";
                    isValidLicence = false;
                }
                else if (_propertiesConfig.ValidarEmpleados(empleadosTotal) == (int)eTipoErrors.ExeceEmpleados)
                {
                    result.Mensaje = "msgLicenciaLimiteEmpleados";
                    isValidLicence = false;
                }
                else if (_propertiesConfig.ValidarEmpleados(empleadosTotal) == (int)eTipoErrors.ExeceUsuarios)
                {
                    result.Mensaje = "msgLicenciaLimiteUsuarios";
                    isValidLicence = false;
                }

                return isValidLicence;
            }

            private async Task<Result> HandleAclaracionesAccess(Empleado dsEmpleado, int claveSistema, Configuracion nombreCliente, IEnumerable<Permisos> permisos)
            {
                var result = new Result();

                result.cvUsuario = dsEmpleado.cveUsuario;
                result.cvEmpleado = dsEmpleado.cveEmpleado;
                result.AdministraPortal = dsEmpleado.bAdministraPortal;

                var procesoAcalaraciones = permisos.FirstOrDefault(item => item.cveProceso == (int)eProcesosMenu.PortalAclaraciones);
                if (procesoAcalaraciones is not null && procesoAcalaraciones.bAutorizar)
                {
                    var TieneMesajesResult = await TieneMensajes(dsEmpleado.cveEmpleado);

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

                return result;
            }

            private bool PermisoPortal(DataTable perfilProceso)
            {

                bool permiso = perfilProceso.AsEnumerable().Any(x =>
                    x.Field<int>("cveProceso") == (int)eProcesosMenu.PortalInicio
                    || x.Field<int>("cveProceso") == (int)eProcesosMenu.PortalAclaraciones);

                return permiso;
            }

        }
    }
}
