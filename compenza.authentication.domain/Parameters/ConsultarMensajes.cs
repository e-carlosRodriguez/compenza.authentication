using System;
using System.Data;

namespace compenza.authentication.domain.Parameters
{
    public class ConsultarMensajes
    {
        public int Accion {get; set;}
        public int cveUsuario {get; set;}
        public DateTime fechaVigencia {get; set;}
        public string? asunto {get; set;} = null;
        public string? textoMensaje {get; set;} = "";
        public int? nivelUrgencia {get; set;} = 0;
        public bool bEstatusEnvio {get; set;} = false;
        public bool bRespuesta {get; set;} = false;
        public string? lblRespuestaSi {get; set;} = "";
        public string? lblRespuestaNo {get; set;} = "";
        public string? lblRespuestaTalVez {get; set;} = "";
        public string? imagenes {get; set;} = "";
        public DataTable? listaEmpleados {get; set;} = null;
        public int cveMensaje {get; set;} = 0;
        public int respuesta {get; set;} = 0;
        public DataTable? listaImagenes { get; set; } = null;
        public string textoProcesado { get; set; } = "";

        public ConsultarMensajes(int Accion, DateTime FechaVijencia, int CveUsuario)
        {
            this.Accion = Accion;
            this.fechaVigencia = FechaVijencia;
            this.cveUsuario = CveUsuario;

            DataTable dtVacia = new DataTable();
            dtVacia.Columns.Add("Campo1");
            dtVacia.Columns.Add("Campo2");
            dtVacia.Columns.Add("Campo3");

            DataTable dtImagenesVacia = new DataTable();
            dtImagenesVacia.Columns.Add("Campo1");
            dtImagenesVacia.Columns.Add("Campo2");
            dtImagenesVacia.Columns.Add("Campo3");
            dtImagenesVacia.Columns.Add("Campo4");

            this.listaEmpleados = dtVacia;
            this.listaImagenes = dtImagenesVacia;
        }
    }
}
