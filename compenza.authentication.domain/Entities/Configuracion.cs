using Dapper.Contrib.Extensions;

namespace compenza.authentication.domain.Entities
{
    [Table("TP_Configuracion")]
    public class Configuracion
    {
        [Key]
        public int cveConfiguracion { get; set; }
        public int ValorEntero { get; set; }
        public string ValorString { get; set; }
    }
}
