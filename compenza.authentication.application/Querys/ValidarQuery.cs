using MediatR;
using Microsoft.Extensions.Options;
using compenza.authentication.domain.Configure;
using compenza.authentication.percistance.Interfaces;
using compenza.authentication.application.Utilities;

namespace compenza.authentication.application.Querys
{
    public class ValidarQuery
    {
        public record Query(
            string LoginName, 
            string Password, 
            string Dominio, 
            string language, 
            bool bEncriptado = false
        ) :IRequest<string>;

        public class Handler : IRequestHandler<Query, string>
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

            public async Task<string> Handle(Query request, CancellationToken cancellationToken)
            {
                var dsEmpleado = await _loginRepository.ConsultarLogin(request.LoginName);
                if ( dsEmpleado is not null )
                {
                    if (dsEmpleado?.bActivo.ToString() == Boolean.TrueString && ( dsEmpleado.bIngresarPortal.ToString() == Boolean.TrueString || Convert.ToInt32(dsEmpleado.cveEmpleado) < 1 ))
                    {
                        var usExterno = false;
                        var proveedor = 0;
                        var Empleado = 0;

                        if (_settings.Debug || ( dsEmpleado.password == _encrypter.EncriptarCadena(request.Password) ) )
                        {
                            
                        }
                    }
                }

                throw new NotImplementedException();
            }
        }
    }
}
