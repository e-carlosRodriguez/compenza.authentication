
namespace compenza.authentication.domain.Enums
{
    public enum eResultado
    {
        Error = 0,
        Redirect = 1,
        Mensaje = 2,
        CambioContra = 3,
        Seleccionar = 4,
        ErrorLicencia = 5
    }

    public enum eProcesosMenu
    {
        PortalInicio = 171,
        PortalAclaraciones = 172,
        InicioObjetivos = 193,
        InicioViajes = 400,
    }

    public enum eSistemasCompenza
    {
        Administrador = 1,
        Portal = 2,
        Objetivos = 3,
        Seguimiento = 4,
        Viajes = 5,
        Historico = 6
    }

    public enum eConfiguracionSistema
    {
        NombreCliente = 48,
        diasCambioPassword = 3,
    }

    public enum eAcciones
    {
        Login = 6
    }
}
