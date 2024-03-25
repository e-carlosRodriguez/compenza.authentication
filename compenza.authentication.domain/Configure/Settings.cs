using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compenza.authentication.domain.Configure
{
    public class Settings
    {
        public string StorageConnectionString { get; set; }
        public string conexion { get; set; }
        public string TipoConexion { get; set; }
        public bool Debug { get; set; }
    }
}
