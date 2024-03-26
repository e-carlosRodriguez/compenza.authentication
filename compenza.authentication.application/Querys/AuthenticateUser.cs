using MediatR;
using Microsoft.Extensions.Options;
using compenza.authentication.domain.Enums;
using compenza.authentication.domain.Configure;
using compenza.authentication.percistance.Interfaces;
using compenza.authentication.application.Utilities;
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
            private readonly Settings _settings;

            public Handler(ILoginRepository loginRepository, IOptions<Settings> Settings, IStringsEncrypter encrypter)
            {
                _loginRepository = loginRepository;
                _settings = Settings.Value;
                _encrypter = encrypter;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = new Result();
                var dsEmpleado = await _loginRepository.ConsultarLogin(request.LoginName);
                if ( dsEmpleado is not null )
                {
                    if (dsEmpleado.bActivo && ( dsEmpleado.bIngresaPortal  || Convert.ToInt32(dsEmpleado.cveEmpleado) < 1 ))
                    {
                        if (_settings.Debug || ( dsEmpleado.password == _encrypter.EncriptarCadena(request.Password) ) )
                        {
                            if ( dsEmpleado.cveUsuario > 0 )
                            {
                                //var cambiarPassword = DateTime.Parse(dsEmpleado.cambiarPassword);
                                //var diasCambio = await _loginRepository.ObtenerConfiguracion(2);
                                //pasos de usuarios externos
                            }

                            var cvePerfil = -1;
                            int.TryParse(dsEmpleado.cvePerfil, out cvePerfil);
                            var permisos = await _loginRepository.ListarPermisos(0, cvePerfil);

                            var permisosPortal = permisos.Any( p => p.cveProceso == (int)eProcesosMenu.PortalInicio || p.cveProceso == (int)eProcesosMenu.PortalAclaraciones );

                            if (permisosPortal)
                            {
                                result.Mensaje = "login";
                                return result;
                            }
                        }
                    }

                    result.Mensaje = dsEmpleado.bActivo ? "Sin acceso al portal." : "Usuario inactivo.";
                    result.Res = false;
                    result.Objeto = (int)eResultado.Error;

                    throw new HttpException(System.Net.HttpStatusCode.BadRequest, "Bad request" , result );
                }

                result.Mensaje = "Usuario y/o contraseña incorrectos";
                result.Res = false;
                result.Objeto = (int)eResultado.Error;

                throw new HttpException(System.Net.HttpStatusCode.BadRequest, "Bad request", result);
            }
        }
    }
}
