using compenza.authentication.api.Payloads.Request;
using compenza.authentication.api.Payloads.Response;
using compenza.authentication.application.Exceptions;
using compenza.authentication.application.Querys;
using compenza.authentication.domain.Configure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

            try
            {
                dllpath = Path.Combine(directorypath, "License.dll");

                if (!System.IO.File.Exists(dllpath))
                {
                    return BadRequest("Sin Licencia");
                }

                dllbytes = System.IO.File.ReadAllBytes(dllpath);
                Assembly loadAssembly = Assembly.Load(dllbytes);

                Type type = loadAssembly.GetType("License.PropertiesConfig");

                if (type is null)
                {
                    return BadRequest("Licencia Invalida");
                }

                object obj = Activator.CreateInstance(type);

                if (obj is null)
                {
                    return BadRequest("Licencia Invalida");
                }

                MethodInfo method = type.GetMethod("Information");

                if (method is null)
                {
                    return BadRequest("Licencia Invalida");
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
            
            if (file == null || file.Length == 0 || file.FileName != fileName)
            {
                return BadRequest(new
                {
                    mensaje = "Archivo no cargado."
                });
            }

            try
            {
                var strPath = HttpContext;
                byte[] dllbytes = null;

                using (var memorystream = new MemoryStream())
                {
                    await file.CopyToAsync(memorystream);
                    dllbytes = memorystream.ToArray();
                }

                Assembly loadAssembly = Assembly.Load(dllbytes);

                Type type = loadAssembly.GetType("License.PropertiesConfig");
                object obj = Activator.CreateInstance(type);
                MethodInfo method = type.GetMethod("Information");

                if (type is null || obj is null || method is null)
                {
                    return BadRequest(new
                    {
                        mensaje = "Licencia Invalida",                     
                    });
                }

                #region ValidacionesPrevioAAgregarNuevaLicencia
                //usar este espacio por si es necesario agregar alguna regla o validacion antes de eliminar o agregar una nueva licencia
                var isValidLicense = await _mediator.Send( new ValidateLicenseBeforeUploading.Query(strPath) );
                if ( !isValidLicense.Res )
                return BadRequest( new
                {
                    mensaje = isValidLicense
                });
                #endregion

                var location = Assembly.GetExecutingAssembly().Location;

                var directorypath = Path.GetDirectoryName(location);

                var dllpath = string.Empty;

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

            return Ok(new
            {
                mensaje = "Licencia cargada exitosamente."
            });
        }

        [HttpPost]
        [Route(nameof(ObtenerReglas))]
        public async Task<IActionResult> ObtenerReglas([FromBody] ObtenerPermisosRequest request)
        {
            try
            {
                var result = await _mediator.Send( new CargarPermisosPorCveProceso.Query( request.CveProceso, request.CvePerfil, request.CveUsuario ) );
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
                var result = await _mediator.Send( new ValidateLicenseBeforeUploading.Query(strPath));
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
