using Microsoft.Azure.Cosmos;
using PhotoCoop.Domain.Users;
using DomainUser = PhotoCoop.Domain.Users.User;

namespace PhotoCoop.Infrastructure.Cosmos.Users;

public class UserRepository : CosmosRepositoryBase<DomainUser>, IUserRepository
{
    public UserRepository(CosmosClientFactory factory)
        : base(factory.UsersContainer)
    {
    }

    public async Task<DomainUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<DomainUser>(
            new QueryDefinition("SELECT * FROM c WHERE c.email = @email")
                .WithParameter("@email", email));

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            var user = response.FirstOrDefault();
            if (user != null) return user;
        }

        return null;
    }

    public async Task<IReadOnlyList<DomainUser>> GetPhotographersByPincodeAndOccasionAsync(
        string pincode,
        OccasionType occasion,
        CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<DomainUser>(
            new QueryDefinition(@"
                SELECT * FROM c
                WHERE c.userType = @photographerType
                AND ARRAY_CONTAINS(c.pincodes, @pincode)
                AND ARRAY_CONTAINS(c.photographerProfile.occasions, @occasion)
            ")
            .WithParameter("@photographerType", (int)UserType.Photographer)
            .WithParameter("@pincode", pincode)
            .WithParameter("@occasion", (int)occasion)
        );

        var results = new List<DomainUser>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    // ✅ NEW: mapped admin query
    public async Task<IReadOnlyList<DomainUser>> GetPhotographersByMappedAdminAsync(
        string mappedAdminUserId,
        CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<DomainUser>(
            new QueryDefinition(@"
                SELECT * FROM c
                WHERE c.userType = @photographerType
                  AND c.photographerProfile.mappedAdminUserId = @mappedAdminUserId
            ")
            .WithParameter("@photographerType", (int)UserType.Photographer)
            .WithParameter("@mappedAdminUserId", mappedAdminUserId)
        );

        var results = new List<DomainUser>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }
        return results;
    }

    // ✅ NEW: mapped admin + pincode filter (optional, but useful)
    public async Task<IReadOnlyList<DomainUser>> GetPhotographersByMappedAdminAndPincodeAsync(
        string mappedAdminUserId,
        string pincode,
        CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<DomainUser>(
            new QueryDefinition(@"
                SELECT * FROM c
                WHERE c.userType = @photographerType
                  AND c.photographerProfile.mappedAdminUserId = @mappedAdminUserId
                  AND ARRAY_CONTAINS(c.pincodes, @pincode)
            ")
            .WithParameter("@photographerType", (int)UserType.Photographer)
            .WithParameter("@mappedAdminUserId", mappedAdminUserId)
            .WithParameter("@pincode", pincode)
        );

        var results = new List<DomainUser>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }
        return results;
    }
}
