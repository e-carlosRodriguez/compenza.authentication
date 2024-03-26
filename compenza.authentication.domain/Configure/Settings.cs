
namespace compenza.authentication.domain.Configure
{
    public class Conexion 
    {
        public string AXO { get; set; }
    }

    public class Settings
    {
        public string StorageConnectionString { get; set; }
        public Conexion conexion { get; set; }
        public string TipoConexion { get; set; }
        public bool Debug { get; set; }
    }
}
