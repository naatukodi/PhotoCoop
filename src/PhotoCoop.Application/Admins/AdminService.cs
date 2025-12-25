
using PhotoCoop.Domain.Users;

namespace PhotoCoop.Application.Admins;

public interface IAdminService
{
    Task<IReadOnlyList<User>> GetMappedPhotographersAsync(
        string adminUserId,
        string? pincode = null,
        CancellationToken cancellationToken = default);

    // ✅ NEW
    Task<User> MapPhotographerToAdminAsync(MapPhotographerRequest request, CancellationToken cancellationToken = default);


}

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;

    public AdminService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<User>> GetMappedPhotographersAsync(
        string adminUserId,
        string? pincode = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pincode))
        {
            return await _userRepository.GetPhotographersByMappedAdminAsync(adminUserId, cancellationToken);
        }

        return await _userRepository.GetPhotographersByMappedAdminAndPincodeAsync(adminUserId, pincode, cancellationToken);
    }

    public async Task<User> MapPhotographerToAdminAsync(MapPhotographerRequest request, CancellationToken cancellationToken = default)
    {
        // validate admin exists and is admin/manager
        var admin = await _userRepository.GetByIdAsync(request.AdminUserId, cancellationToken);
        if (admin == null || (admin.UserType != UserType.Admin && admin.UserType != UserType.Manager))
            throw new InvalidOperationException("Invalid admin/manager.");

        // validate photographer exists
        var photographer = await _userRepository.GetByIdAsync(request.PhotographerUserId, cancellationToken);
        if (photographer == null || photographer.UserType != UserType.Photographer || photographer.PhotographerProfile == null)
            throw new InvalidOperationException("Invalid photographer.");

        // Map in domain
        photographer.PhotographerProfile.MapToAdmin(request.AdminUserId, request.MappedByUserId);

        return await _userRepository.UpdateAsync(photographer, cancellationToken);
    }
}
