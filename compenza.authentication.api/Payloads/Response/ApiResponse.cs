namespace compenza.authentication.api.Payloads.Response
{
    public class ApiResponse<T>
    {
        public bool Res { get; set; }
        public string Mensaje { get; set; }
        public T Objeto  { get; set; }

        public ApiResponse(bool res, string mensaje, T objeto) 
        { 
            this.Res = res;
            this.Mensaje = mensaje;
            this.Objeto = objeto;
        }
    }
}
