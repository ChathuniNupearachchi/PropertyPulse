using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyPulse.Api.Services;
using PropertyPulse.Domain.Enums;
using PropertyPulse.Shared.Auth;

namespace PropertyPulse.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = nameof(UserRole.Manager))]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>Lists all users. Managers only.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> List(CancellationToken cancellationToken) =>
        Ok(await userService.ListAsync(cancellationToken));
}
