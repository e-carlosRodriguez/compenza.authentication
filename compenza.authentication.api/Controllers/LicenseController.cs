using compenza.authentication.api.Payloads.Request;
using compenza.authentication.api.Payloads.Response;
using compenza.authentication.application.Exceptions;
using compenza.authentication.application.Querys;
using compenza.authentication.domain.Configure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Reflection;

namespace compenza.authentication.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseController : ControllerBase
    {
        private const string fileName = "License.dll";

        private readonly IMediator _mediator;

        public LicenseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public ActionResult GetLicense()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var directorypath = Path.GetDirectoryName(location);
            var dllpath = string.Empty;
            byte[] dllbytes = null;
            var result = new Result();
            var apiResponse = new ApiResponse<Result>(result);

            try
            {
                dllpath = Path.Combine(directorypath, "License.dll");

                if (!System.IO.File.Exists(dllpath))
                {
                    result.Mensaje = "Sin Licencia";
                    result.Objeto = (int)eTipoErrors.SinLicencia;
                    result.Res = false;
                    apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    apiResponse.Message = "Error";

                    return Ok(apiResponse);
                }

                dllbytes = System.IO.File.ReadAllBytes(dllpath);
                Assembly loadAssembly = Assembly.Load(dllbytes);

                Type type = loadAssembly.GetType("License.PropertiesConfig");
                object obj = Activator.CreateInstance(type);
                MethodInfo method = type.GetMethod("Information");

                if (type is null || obj is null || method is null)
                {
                    result.Mensaje = "Sin Licencia";
                    result.Objeto = (int)eTipoErrors.ErrorArchivo;
                    result.Res = false;
                    apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    apiResponse.Message = "Error";

                    return BadRequest(apiResponse);
                }

                var res = method.Invoke(obj, Array.Empty<object>());

                return Ok(JsonConvert.SerializeObject(res));
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

        [HttpPost(Name = "Upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var strPath = HttpContext;

            byte[] dllbytes = null;

            var dllpath = string.Empty;

            var result = new Result();

            var apiResponse = new ApiResponse<Result>(result);

            if (file == null || file.Length == 0 || file.FileName != fileName)
            {
                result.Mensaje = "Archivo no cargado.";
                result.Objeto = (int)eTipoErrors.ErrorArchivo;
                result.Res = false;
                apiResponse.StatusCode = HttpStatusCode.BadRequest;
                apiResponse.Message = "Ok";

                return Ok(apiResponse);
            }

            try
            {
                var location = Assembly.GetExecutingAssembly().Location;

                var directorypath = Path.GetDirectoryName(location);

                dllpath = Path.Combine(directorypath, "License.dll");

                if (System.IO.File.Exists(dllpath))
                {
                    System.IO.File.Delete(dllpath);
                }

                using (var filestream = new FileStream(dllpath, FileMode.Create))
                {
                    await file.CopyToAsync(filestream);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{ex.Message}");
            }
            finally
            {
                if (dllbytes != null)
                {
                    Array.Clear(dllbytes, 0, dllbytes.Length);
                    dllbytes = null;
                }
            }

            #region ValidacionesPrevioAAgregarNuevaLicencia
            //usar este espacio por si es necesario agregar alguna regla o validacion antes de eliminar o agregar una nueva licencia
            var isValidLicense = await _mediator.Send(new ValidateLicenseBeforeUploading.Query(strPath));
            #endregion

            DataSet LogoCompenza = await _mediator.Send(new ObtenerConfiguracionesPorId.Query(4, 20));

            if (LogoCompenza.Tables.Count > 0 && LogoCompenza.Tables[0].Rows.Count > 0)
            {
                string logoCompenza = LogoCompenza.Tables[0].Rows[0]["ValorString"].ToString();

                result.LogoCompenza = logoCompenza;
            }

            DataSet ImagenBackgroundCompenza = await _mediator.Send(new ObtenerConfiguracionesPorId.Query(4, 21));

            if (ImagenBackgroundCompenza.Tables.Count > 0 && ImagenBackgroundCompenza.Tables[0].Rows.Count > 0)
            {
                string imagenBackgroundCompenza = ImagenBackgroundCompenza.Tables[0].Rows[0]["ValorString"].ToString();

                result.ImagenBackgroundCompenza = imagenBackgroundCompenza;
            }

            if (!isValidLicense.Res)
            {
                System.IO.File.Delete(dllpath);

                result.Mensaje = result.Mensaje;
                result.Objeto = isValidLicense.Objeto;
                result.Res = isValidLicense.Res;
                apiResponse.StatusCode = HttpStatusCode.BadRequest;
                apiResponse.Message = isValidLicense.Mensaje;
                result.Actualizarlicencia = isValidLicense.Actualizarlicencia;

                return BadRequest(apiResponse);
            }

            result.Mensaje = "Licencia cargada exitosamente.";
            result.Objeto = (int)eTipoErrors.LicenciaValida;
            result.Res = true;
            apiResponse.StatusCode = HttpStatusCode.OK;
            apiResponse.Message = "Ok";
            result.Actualizarlicencia = isValidLicense.Actualizarlicencia;

            return Ok(apiResponse);

        }

        [HttpPost]
        [Route(nameof(ObtenerReglas))]
        public async Task<IActionResult> ObtenerReglas([FromBody] ObtenerPermisosRequest request)
        {
            try
            {
                var result = await _mediator.Send(new CargarPermisosPorCveProceso.Query(request.CveProceso, request.CvePerfil, request.CveUsuario));
                var apiResponse = new ApiResponse<Result>(result);

                return Ok(apiResponse);
            }
            catch (HttpException e)
            {
                throw new HttpException(e.StatusCode, e.Message, e.Errors);
            }
        }

        [HttpGet(nameof(ValidarLicencia))]
        public async Task<IActionResult> ValidarLicencia()
        {
            try
            {
                var strPath = HttpContext;
                var result = await _mediator.Send(new ValidateLicenseBeforeUploading.Query(strPath));

                DataSet LogoCompenza = await _mediator.Send(new ObtenerConfiguracionesPorId.Query(4, 20));

                if (LogoCompenza.Tables.Count > 0 && LogoCompenza.Tables[0].Rows.Count > 0)
                {
                    string logoCompenza = LogoCompenza.Tables[0].Rows[0]["ValorString"].ToString();

                    result.LogoCompenza = logoCompenza;
                }

                DataSet ImagenBackgroundCompenza = await _mediator.Send(new ObtenerConfiguracionesPorId.Query(4, 21));

                if (ImagenBackgroundCompenza.Tables.Count > 0 && ImagenBackgroundCompenza.Tables[0].Rows.Count > 0)
                {
                    string imagenBackgroundCompenza = ImagenBackgroundCompenza.Tables[0].Rows[0]["ValorString"].ToString();

                    result.ImagenBackgroundCompenza = imagenBackgroundCompenza;
                }

                if (!result.Res)
                    return BadRequest(result);

                return Ok(result);

            }
            catch (HttpException e)
            {
                throw new HttpException(e.StatusCode, e.Message, e.Errors);
            }
        }
    }
}
