using PhotoCoop.Domain.Common;
using PhotoCoop.Domain.Users;

namespace PhotoCoop.Domain.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetPhotographersByPincodeAndOccasionAsync(
        string pincode,
        OccasionType occasion,
        CancellationToken cancellationToken = default);

    // ✅ NEW: fetch photographers mapped to a specific admin/manager
    Task<IReadOnlyList<User>> GetPhotographersByMappedAdminAsync(
        string mappedAdminUserId,
        CancellationToken cancellationToken = default);

    // ✅ Optional: admin + pincode filter (useful for dashboards)
    Task<IReadOnlyList<User>> GetPhotographersByMappedAdminAndPincodeAsync(
        string mappedAdminUserId,
        string pincode,
        CancellationToken cancellationToken = default);
}
