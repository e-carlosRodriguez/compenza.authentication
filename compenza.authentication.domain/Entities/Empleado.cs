﻿

namespace compenza.authentication.domain.Entities
{
    public class Empleado
    {
        public int cveUsuario { get; set; }
        public string Nombre { get; set; }
        public string eMail { get; set; }
        public string cvePerfil { get; set; }
        public string password { get; set; }
        public int cveEmpleado { get; set; }
        public bool bActivo { get; set; }
        public string cambiarPassword { get; set; }
        public bool bAdministrarPortal { get; set; }
        public bool bIngresaPortal { get; set; }
        public string idioma { get; set; }
        public int? cveClaveCova { get; set; }
        public bool? UsExterno { get; set; }
        public int? cveProveedor { get; set; }
    }
}
