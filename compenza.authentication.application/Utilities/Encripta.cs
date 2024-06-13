using eSoftware.Utilerias;

namespace compenza.authentication.application.Utilities
{
    public class Encripta
    {
        private static string sIV = "CanalBI_Net_v2.2";
        private static string sKey = "e-Software&BuSol";

        public Encripta() { }

        public static string EncriptarCadena(string strCadena)
        {
            Crypto objCrypto = new Crypto();
            objCrypto.Algoritmo = CryptoProvider.TripleDES;
            objCrypto.Key = Encripta.sKey;
            objCrypto.IV = Encripta.sIV;
            return objCrypto.CifrarCadena(strCadena);
        }

        public static string DesencriptarCadena(string strCadena)
        {
            Crypto objCrypto = new Crypto();
            objCrypto.Algoritmo = CryptoProvider.TripleDES;
            objCrypto.Key = Encripta.sKey;
            objCrypto.IV = Encripta.sIV;
            return objCrypto.DescifrarCadena(strCadena);
        }
    }
}
