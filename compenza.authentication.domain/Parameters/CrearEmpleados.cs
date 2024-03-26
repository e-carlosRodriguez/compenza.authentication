

namespace compenza.authentication.domain.Parameters
{
    public class CrearEmpleados
    {
        public int Accion { get; set; }
        public int? cveEmpleado { get; set; }
        public string? Nombre { get; set; }
        public string? loginUser { get; set; }
        public string? eMail { get; set; }

        public CrearEmpleados(int Accion, int cveEmpleado = 0, string? nombre = null, string? loginUser = null, string? email = null)
        {
            this.Accion = Accion;
            this.Nombre = nombre;
            this.eMail = email;
            this.loginUser = loginUser;
            this.cveEmpleado = cveEmpleado;
        }
    }
}
