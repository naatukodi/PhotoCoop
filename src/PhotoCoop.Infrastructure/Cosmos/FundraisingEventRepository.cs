using Microsoft.Azure.Cosmos;
using PhotoCoop.Domain.Fundraising;
using PhotoCoop.Infrastructure.Cosmos;

namespace PhotoCoop.Infrastructure.Cosmos.Fundraising;

public class FundraisingEventRepository : CosmosRepositoryBase<FundraisingEvent>, IFundraisingEventRepository
{
    public FundraisingEventRepository(CosmosClientFactory factory)
        : base(factory.FundraisingEventsContainer)
    {
    }

    public async Task<IReadOnlyList<FundraisingEvent>> GetActiveEventsAsync(CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<FundraisingEvent>(
            new QueryDefinition("SELECT * FROM c WHERE c.status = @status")
                .WithParameter("@status", (int)FundraisingEventStatus.Active));

        var results = new List<FundraisingEvent>();
        while (query.HasMoreResults)
        {
            var resp = await query.ReadNextAsync(cancellationToken);
            results.AddRange(resp);
        }
        return results;
    }
}
