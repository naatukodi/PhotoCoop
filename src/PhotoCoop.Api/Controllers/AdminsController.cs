using Microsoft.AspNetCore.Mvc;
using PhotoCoop.Application.Admins;

namespace PhotoCoop.Api.Controllers;

[ApiController]
[Route("api/admins")]
public class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // GET /api/admins/{adminUserId}/photographers?pincode=xxxxx
    [HttpGet("{adminUserId}/photographers")]
    public async Task<IActionResult> GetPhotographersForAdmin(
        string adminUserId,
        [FromQuery] string? pincode,
        CancellationToken cancellationToken)
    {
        var photographers = await _adminService.GetMappedPhotographersAsync(adminUserId, pincode, cancellationToken);
        return Ok(photographers);
    }

    // ✅ POST /api/admins/{adminUserId}/photographers/{photographerUserId}/map
    [HttpPost("{adminUserId}/photographers/{photographerUserId}/map")]
    public async Task<IActionResult> MapPhotographer(
        string adminUserId,
        string photographerUserId,
        [FromBody] MapPhotographerRequest body,
        CancellationToken cancellationToken)
    {
        // Keep route as source of truth
        body.AdminUserId = adminUserId;
        body.PhotographerUserId = photographerUserId;

        var updatedPhotographer = await _adminService.MapPhotographerToAdminAsync(body, cancellationToken);
        return Ok(updatedPhotographer);
    }
}
