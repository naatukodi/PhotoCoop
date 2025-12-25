namespace PhotoCoop.Application.Admins;

public class MapPhotographerRequest
{
    public string AdminUserId { get; set; } = null!;
    public string PhotographerUserId { get; set; } = null!;

    // who performed the action (superadmin/admin). Often same as AdminUserId.
    public string MappedByUserId { get; set; } = null!;
}
