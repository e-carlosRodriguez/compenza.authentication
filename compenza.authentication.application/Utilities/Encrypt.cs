using compenza.identity.domain.Configuration;
using eSoftware.Utilerias;
using Microsoft.Extensions.Options;

namespace compenza.authentication.application.Utilities
{
    public class Encrypt : IStringsEncrypter
    {
        private EncryptSecrets _secrets;

        public Encrypt(IOptions<EncryptSecrets> SecretOption)
        {
            _secrets = SecretOption.Value;
        }

        public string DesencriptarCadena(string strCadena)
        {
            Crypto objCrypto = new Crypto();
            objCrypto.Algoritmo = CryptoProvider.TripleDES;
            objCrypto.Key = _secrets.SKey;
            objCrypto.IV = _secrets.SIV;
            return objCrypto.DescifrarCadena(strCadena);
        }

        public string EncriptarCadena(string strCadena)
        {
            Crypto objCrypto = new Crypto();
            objCrypto.Algoritmo = CryptoProvider.TripleDES;
            objCrypto.Key = _secrets.SKey;
            objCrypto.IV = _secrets.SIV;
            return objCrypto.CifrarCadena(strCadena);
        }
    }
}
