using MediatR;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using compenza.authentication.domain.Configure;
using compenza.authentication.application.Querys;
using compenza.authentication.api.Payloads.Request;
using compenza.authentication.api.Payloads.Response;
using compenza.authentication.application.Exceptions;

namespace compenza.authentication.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Validar([FromBody] LoginCredentialsRequest request)
        {
            try
            {
                var result = await _mediator.Send(new AuthenticateUser.Query(request.LoginName, request.Password, "", ""));

                var apiResponse = new ApiResponse<Result>(result);
                apiResponse.StatusCode = HttpStatusCode.OK;
                apiResponse.Message = "Ok";

                return Ok(apiResponse);
            }
            catch (HttpException e)
            {
                throw new HttpException(e.StatusCode, e.Message, e.Errors);
            }
        }
    }
}
