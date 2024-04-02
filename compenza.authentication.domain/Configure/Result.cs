using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compenza.authentication.domain.Configure
{
    public class Result
    {
        public string Mensaje { get; set; }
        public bool Res { get; set; } = true;
        public object Objeto { get; set; }
        public string? Redirect { get; set; }
        public string Token{ get; set; }
    }
}
