using Microsoft.Azure.Cosmos;
using PhotoCoop.Domain.Bookings;

namespace PhotoCoop.Infrastructure.Cosmos.Bookings;

public class BookingRepository : CosmosRepositoryBase<Booking>, IBookingRepository
{
    public BookingRepository(CosmosClientFactory factory)
        : base(factory.BookingsContainer)
    {
    }

    public override async Task<Booking?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<Booking>(
            new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                .WithParameter("@id", id),
            requestOptions: new QueryRequestOptions { MaxItemCount = 1 });

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            var booking = response.FirstOrDefault();
            if (booking != null) return booking;
        }

        return null;
    }

    public override async Task<Booking> AddAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        var response = await _container.CreateItemAsync(
            entity,
            new PartitionKey(entity.CustomerUserId),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public override async Task<Booking> UpdateAsync(Booking entity, CancellationToken cancellationToken = default)
    {
        var response = await _container.UpsertItemAsync(
            entity,
            new PartitionKey(entity.CustomerUserId),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public override async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var booking = await GetByIdAsync(id, cancellationToken);
        if (booking == null)
            return;

        await _container.DeleteItemAsync<Booking>(
            id,
            new PartitionKey(booking.CustomerUserId),
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetByCustomerAsync(
        string customerUserId,
        CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<Booking>(
            new QueryDefinition("SELECT * FROM c WHERE c.customerUserId = @customerUserId")
                .WithParameter("@customerUserId", customerUserId));

        var results = new List<Booking>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<IReadOnlyList<Booking>> GetByPhotographerAsync(
        string photographerUserId,
        CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<Booking>(
            new QueryDefinition("SELECT * FROM c WHERE c.assignedPhotographerUserId = @photographerUserId")
                .WithParameter("@photographerUserId", photographerUserId));

        var results = new List<Booking>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }
}
