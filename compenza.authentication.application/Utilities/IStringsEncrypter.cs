using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compenza.authentication.application.Utilities
{

    public interface IStringsEncrypter
    {
        string EncriptarCadena(string strCadena);
        string DesencriptarCadena(string strCadena);
    }
}
