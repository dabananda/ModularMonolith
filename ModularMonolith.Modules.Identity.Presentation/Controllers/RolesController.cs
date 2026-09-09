using ModularMonolith.Modules.Identity.Application.Features.Role.Assign;
using ModularMonolith.Modules.Identity.Application.Features.Role.Create;
using ModularMonolith.Modules.Identity.Application.Features.Role.Delete;
using ModularMonolith.Modules.Identity.Application.Features.Role.GetAll;
using ModularMonolith.Modules.Identity.Application.Features.Role.GetById;
using ModularMonolith.Modules.Identity.Application.Features.Role.Remove;
using ModularMonolith.Modules.Identity.Application.Features.Role.Update;
using ModularMonolith.Modules.Identity.Domain.Constants;
using ModularMonolith.Shared.Controllers;
using ModularMonolith.Shared.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ModularMonolith.Modules.Identity.Presentation.Controllers
{
    [Authorize(Roles = $"{Roles.Admin},{Roles.SuperAdmin}")]
    public class RolesController(ISender sender) : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(new GetRoleByIdQuery(id), cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllRolesQuery query, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(query, cancellationToken));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRoleCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(new DeleteRoleCommand(id), cancellationToken));
        }

        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignRoleCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] RemoveRoleCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }
    }
}