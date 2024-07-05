namespace compenza.authentication.domain.Configure
{
    public class Parameters
    {
        public string ID { get; set; }
        public int Alerta { get; set; }
        public int Expira { get; set; }
        public DateTime FechaLicencia { get; set; }
        public int Empleados { get; set; }
        public double? Etca { get; set; }
        public int Usuarios { get; set; }
        public double? Utca { get; set; }
        public int Dimensiones { get; set; }
        public string ServerID { get; set; }
        public string BDServerID { get; set; }
        public string ServerPortalID { get; set; }
        public string HistoricServerID { get; set; }
        public string BDHistoricServerID { get; set; }
        public List<Modulo> Modulos { get; set; }
        public List<Regla> Reglas { get; set; }
    }

    public class Modulo
    {
        public int Id { get; set; }
        public bool activate { get; set; }
    }

    public class Regla
    {
        public int Id { get; set; }
        public int Valor { get; set; }
    }
}
