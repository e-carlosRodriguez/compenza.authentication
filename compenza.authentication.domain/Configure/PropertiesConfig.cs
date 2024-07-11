using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace compenza.authentication.domain.Configure
{
    public class PropertiesConfig
    {
        public async Task<Parameters> Information()
        {
            var location = Assembly.GetExecutingAssembly().Location;

            var directorypath = Path.GetDirectoryName(location);

            var dllpath = string.Empty;

            byte[] dllbytes = null;

            try
            {
                dllpath = Path.Combine(directorypath, "License.dll");

                if (!System.IO.File.Exists(dllpath))
                {
                    return new Parameters();
                }

                dllbytes = System.IO.File.ReadAllBytes(dllpath);

                Assembly loadAssembly = Assembly.Load(dllbytes);

                Type type = loadAssembly.GetType("License.PropertiesConfig");

                object obj = Activator.CreateInstance(type);

                MethodInfo method = type.GetMethod("Information");

                if (type is null || obj is null || method is null)
                {
                    object errorDetails = new { AdditionalInfo = "Licencia Invalida." };
                    throw new HttpException(System.Net.HttpStatusCode.Locked, "Error Licencia Invalida:", errorDetails);
                }

                var res = method.Invoke(obj, Array.Empty<object>());

                if (res == null)
                {
                    throw new InvalidOperationException("El método Information no devolvió una lista válida de Parametros.");
                }

                string json = JsonConvert.SerializeObject(res);

                Parameters param = JsonConvert.DeserializeObject<Parameters>(json);

                return param;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (dllbytes != null)
                {
                    Array.Clear(dllbytes, 0, dllbytes.Length);
                    dllbytes = null;
                }
            }

        }

        public async Task<int> ValidarFechas()
        {
            Parameters _parameters = new Parameters();

            _parameters = await Information();

            DateTime Hoy = DateTime.Today;
            DateTime FechaLicencia = _parameters.FechaLicencia;
            DateTime FechaAlertas = FechaLicencia.AddDays(-_parameters.Alerta);
            DateTime FechaCierre = FechaLicencia.AddDays(_parameters.Expira);
            if (Hoy.CompareTo(FechaCierre) >= 0)
                return (int)eTipoErrors.LicenciaVencida;
            else if (Hoy.CompareTo(FechaLicencia) > 0)
                return (int)eTipoErrors.LicenciaVencidaAlerta;
            else if (Hoy.CompareTo(FechaLicencia) == 0)
                return (int)eTipoErrors.LicenciaUltimoDiaconLicencia;
            else if (Hoy.CompareTo(FechaAlertas) >= 0)
                return (int)eTipoErrors.LicenciasAlertas;
            return (int)eTipoErrors.SinError;
        }

        public async Task<int> ValidarEmpleados(int empleado)
        {
            Parameters _parameters = new Parameters();

            _parameters = await Information();

            double tolerancia = _parameters.Etca ?? 1.05;

            int EmpleadoConfiguracion = _parameters.Empleados;

            int EmpleadosBD = empleado;

            if (EmpleadosBD > (EmpleadoConfiguracion * tolerancia))
                return (int)eTipoErrors.ExeceEmpleados;
            else if (EmpleadosBD > EmpleadoConfiguracion)
                return (int)eTipoErrors.ToleranciaEmpleados;

            return (int)eTipoErrors.SinError;
        }

        public async Task<double> ToleranciaEmpleados()
        {
            Parameters _parameters = new Parameters();

            _parameters = await Information();

            return _parameters.Etca ?? 1.05;
        }

        public async Task<int> ValidarUsuarios(int Usuarios)
        {
            Parameters _parameters = new Parameters();

            _parameters = await Information();

            double tolerancia = _parameters.Utca ?? 1.05;

            int UsuariosConfiguracion = _parameters.Usuarios;

            int UsuariosBD = Usuarios;

            if (UsuariosBD > (UsuariosConfiguracion * tolerancia))
                return (int)eTipoErrors.ExeceUsuarios;
            else if (UsuariosBD > UsuariosConfiguracion)
                return (int)eTipoErrors.ToleranciaEmpleados;

            return (int)eTipoErrors.SinError;

        }

        public async Task<double> ToleranciaUsuarios()
        {
            Parameters _parameters = new Parameters();

            _parameters = await Information();

            return _parameters.Utca ?? 1.05;
        }

        public async Task<int> ValidarModulo(int idModulo)
        {
            Parameters _parameters = new Parameters();

            _parameters = await Information();

            List<Modulo> modulos = _parameters.Modulos;
            var modulo = modulos.Find(x => x.Id == idModulo);
            if (modulo != null)
                return (int)eTipoErrors.SinError;
            return (int)eTipoErrors.ModuloNoEncontrado;
        }

        public async Task<int> ValidarModulo(int idModulo, Modulo modulo)
        {
            Parameters _parameters = new Parameters();

            _parameters = await Information();

            List<Modulo> modulos = _parameters.Modulos;

            modulo = modulos.Find(x => x.Id == idModulo);

            if (modulo != null)
                return (int)eTipoErrors.SinError;

            return (int)eTipoErrors.ModuloNoEncontrado;
        }

        public async Task<int> ValidarServer(string BDServerIDConfiguration, string ServerIDConfiguration)
        {
            Parameters _parameters = new Parameters();

            _parameters = await Information();

            var serverID = _parameters.ServerID;
            var bdID = _parameters.BDServerID;

            if (serverID != ServerIDConfiguration || bdID != BDServerIDConfiguration)
            {
                return (int)eTipoErrors.ErrorArchivo;
            }
            return (int)eTipoErrors.SinError;
        }
    }

    public enum eTipoErrors
    {
        SinError = 0,
        LicenciasAlertas = 1,
        LicenciaUltimoDiaconLicencia = 2,
        LicenciaVencidaAlerta = 3,
        LicenciaVencida = 4,
        ExeceEmpleados = 5,
        ExeceUsuarios = 6,
        ExecedeDimensiones = 7,
        ModuloNoEncontrado = 8,
        ErrorArchivo = 9,
        ActualizarLicencia = 10,
        ToleranciaEmpleados = 11,
        ToleranciaUsuarios = 12,
        SinCompenzaHistorico = 13,
        LicenciaValida = 14,
        ServidorInvalido = 15,
        SinLicencia = 16
    }
}
