using Microsoft.Azure.Cosmos;
using PhotoCoop.Domain.Fundraising;
using PhotoCoop.Infrastructure.Cosmos;

namespace PhotoCoop.Infrastructure.Cosmos.Fundraising;

public class DonationAttemptRepository : CosmosRepositoryBase<DonationAttempt>, IDonationAttemptRepository
{
    public DonationAttemptRepository(CosmosClientFactory factory)
        : base(factory.DonationAttemptsContainer)
    {
    }

    public async Task<DonationAttempt?> GetByRazorpayOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
    {
        var q = _container.GetItemQueryIterator<DonationAttempt>(
            new QueryDefinition("SELECT * FROM c WHERE c.razorpayOrderId = @orderId")
                .WithParameter("@orderId", orderId));

        while (q.HasMoreResults)
        {
            var resp = await q.ReadNextAsync(cancellationToken);
            var item = resp.Resource.FirstOrDefault();
            if (item != null) return item;
        }
        return null;
    }

    public async Task<DonationAttempt?> GetByReceiptAsync(string receipt, CancellationToken cancellationToken = default)
    {
        var q = _container.GetItemQueryIterator<DonationAttempt>(
            new QueryDefinition("SELECT * FROM c WHERE c.receipt = @receipt")
                .WithParameter("@receipt", receipt));

        while (q.HasMoreResults)
        {
            var resp = await q.ReadNextAsync(cancellationToken);
            var item = resp.Resource.FirstOrDefault();
            if (item != null) return item;
        }
        return null;
    }

    public async Task<IReadOnlyList<DonationAttempt>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var q = _container.GetItemQueryIterator<DonationAttempt>(
            new QueryDefinition("SELECT * FROM c WHERE c.eventId = @eventId")
                .WithParameter("@eventId", eventId));

        var results = new List<DonationAttempt>();
        while (q.HasMoreResults)
        {
            var resp = await q.ReadNextAsync(cancellationToken);
            results.AddRange(resp);
        }
        return results;
    }
}
