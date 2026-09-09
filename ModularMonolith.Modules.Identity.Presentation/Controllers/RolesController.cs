using ModularMonolith.Modules.Identity.Application.Features.Role.Create;
using ModularMonolith.Modules.Identity.Application.Features.Role.Delete;
using ModularMonolith.Modules.Identity.Application.Features.Role.GetAll;
using ModularMonolith.Modules.Identity.Application.Features.Role.GetById;
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
        public async Task<IActionResult> Create([FromBody] CreateRoleCommand command)
        {
            return HandleResult(await sender.Send(command));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return HandleResult(await sender.Send(new GetRoleByIdQuery(id)));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllRolesQuery query)
        {
            return HandleResult(await sender.Send(query));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRoleCommand command)
        {
            return HandleResult(await sender.Send(command));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] DeleteRoleCommand command)
        {
            return HandleResult(await sender.Send(command));
        }
    }
}