using compenza.authentication.api.Payloads.Response;
using compenza.authentication.application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace compenza.authentication.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseController : ControllerBase
    {
        [HttpGet]
        public ActionResult GetLicense()
        {
            try
            {
                var license = License.PropertiesConfig.Information();
                var apiResponse = new ApiResponse<License.Parameters>(license);
                return Ok(apiResponse);
            }
            catch (HttpException e)
            {
                throw new HttpException(e.StatusCode, e.Message, e.Errors);
            }
        }
    }
}
