using KrtBank.Api.Wrappers;
using KrtBank.Application.Commands.Accounts;
using KrtBank.Application.Queries.Accounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KrtBank.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/account")]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{cpf}")]
        [ProducesResponseType(typeof(ApiResponse<AccountResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountByCPF([FromRoute] string cpf)
        {
            var result = await _mediator.Send(new GetAccountByCpfQuery(cpf));
            return Ok(ApiResponse<AccountResponse>.Create(result));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AccountResponse>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateAccountCommand command)
        {
            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(Create),
                ApiResponse<AccountResponse>.Create(result, "Conta aberta com sucesso!"));
        }

        [HttpPatch("status")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateAccountStatusCommand command)
        {
            await _mediator.Send(command);
            return Ok(ApiResponse<bool>.Create(true, "Status da conta atualizado com sucesso."));
        }
    }
}
