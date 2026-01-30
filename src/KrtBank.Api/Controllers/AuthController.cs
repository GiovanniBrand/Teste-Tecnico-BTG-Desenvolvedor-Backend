using KrtBank.Api.Wrappers;
using KrtBank.Application.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KrtBank.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<LoginResponse>.Create(result, "Login realizado com sucesso."));
        }
    }
}
