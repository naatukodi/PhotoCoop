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
}
