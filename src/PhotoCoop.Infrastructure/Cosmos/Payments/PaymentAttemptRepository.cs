using Microsoft.Azure.Cosmos;
using PhotoCoop.Domain.Payments;

namespace PhotoCoop.Infrastructure.Cosmos.Payments;

public class PaymentAttemptRepository : IPaymentAttemptRepository
{
    private readonly Container _container;

    public PaymentAttemptRepository(CosmosClientFactory factory)
    {
        _container = factory.PaymentAttemptsContainer;
    }

    public async Task<PaymentAttempt?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        // Need PK to ReadItem fast; fallback to query for simplicity
        var q = _container.GetItemQueryIterator<PaymentAttempt>(
            new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                .WithParameter("@id", id));

        while (q.HasMoreResults)
        {
            var resp = await q.ReadNextAsync(ct);
            var item = resp.FirstOrDefault();
            if (item != null) return item;
        }
        return null;
    }

    public async Task<PaymentAttempt?> GetByRazorpayOrderIdAsync(string razorpayOrderId, CancellationToken ct = default)
    {
        var q = _container.GetItemQueryIterator<PaymentAttempt>(
            new QueryDefinition("SELECT * FROM c WHERE c.razorpayOrderId = @oid")
                .WithParameter("@oid", razorpayOrderId));

        while (q.HasMoreResults)
        {
            var resp = await q.ReadNextAsync(ct);
            var item = resp.FirstOrDefault();
            if (item != null) return item;
        }
        return null;
    }

    public async Task<PaymentAttempt?> GetByRazorpayPaymentIdAsync(string razorpayPaymentId, CancellationToken ct = default)
    {
        var q = _container.GetItemQueryIterator<PaymentAttempt>(
            new QueryDefinition("SELECT * FROM c WHERE c.razorpayPaymentId = @pid")
                .WithParameter("@pid", razorpayPaymentId));

        while (q.HasMoreResults)
        {
            var resp = await q.ReadNextAsync(ct);
            var item = resp.FirstOrDefault();
            if (item != null) return item;
        }
        return null;
    }

    public async Task<PaymentAttempt> AddAsync(PaymentAttempt attempt, CancellationToken ct = default)
    {
        var resp = await _container.CreateItemAsync(attempt, new PartitionKey(attempt.PartitionKey), cancellationToken: ct);
        return resp.Resource;
    }

    public async Task<PaymentAttempt> UpdateAsync(PaymentAttempt attempt, CancellationToken ct = default)
    {
        var resp = await _container.UpsertItemAsync(attempt, new PartitionKey(attempt.PartitionKey), cancellationToken: ct);
        return resp.Resource;
    }

    public async Task<IReadOnlyList<PaymentAttempt>> GetByPhotographerAsync(string photographerUserId, int take = 50, CancellationToken ct = default)
    {
        var q = _container.GetItemQueryIterator<PaymentAttempt>(
            new QueryDefinition("SELECT TOP @take * FROM c WHERE c.partitionKey = @pk ORDER BY c.createdAtUtc DESC")
                .WithParameter("@take", take)
                .WithParameter("@pk", photographerUserId));

        var list = new List<PaymentAttempt>();
        while (q.HasMoreResults)
        {
            var resp = await q.ReadNextAsync(ct);
            list.AddRange(resp);
        }
        return list;
    }
}
