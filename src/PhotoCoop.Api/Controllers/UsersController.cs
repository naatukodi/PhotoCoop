using Microsoft.AspNetCore.Mvc;
using PhotoCoop.Application.Users;

namespace PhotoCoop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateCustomerAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPost("photographers")]
    public async Task<IActionResult> CreatePhotographer([FromBody] CreatePhotographerUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.CreatePhotographerAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPost("admins")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAdminAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        if (user == null) return NotFound();
        return Ok(user);
    }
}
