using System.Text;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using compenza.authentication.domain.Entities;
using compenza.authentication.domain.Configure;

namespace compenza.authentication.application.Utilities
{
    public class TokenProvider : ITokenProvider
    {
        private readonly Settings _settings;
        public TokenProvider(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        public string GetTokenAsync(Empleado emp, int cveSistemas, string nombreClient, IEnumerable<Permisos> permisos)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var MAC = new HMACSHA256(Encoding.ASCII.GetBytes("1qaz-2wsxx-2wxx-2sxxc-2sxwccd-2srdewfwe"));
            var claims = new Dictionary<string, object?>();

            claims.Add("cveSistema", cveSistemas);
            claims.Add("iIdUsuario", emp.cveUsuario);
            claims.Add("sNombre", emp.Nombre);
            claims.Add("eMail", emp.eMail);
            claims.Add("cvePerfil", emp.cvePerfil);
            claims.Add("cveEmpleado", emp.cveEmpleado);
            claims.Add("AdminPortal", emp.bAdministrarPortal);
            claims.Add("lenguaje", emp.idioma == "1" ? "es" : "en");
            claims.Add("verDatosEmpleado", true);
            claims.Add("verNoticias", true);
            claims.Add("nombreCliente", nombreClient);
            claims.Add("cveClaveCova", emp.cveClaveCova is not null ? emp.cveClaveCova : null );
            claims.Add("reglas", "");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(MAC.Key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}
