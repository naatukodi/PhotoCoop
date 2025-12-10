using System.Net;
using Microsoft.Azure.Cosmos;
using PhotoCoop.Domain.Common;

namespace PhotoCoop.Infrastructure.Cosmos;

public abstract class CosmosRepositoryBase<T> : IRepository<T> where T : Entity
{
    protected readonly Container _container;

    protected CosmosRepositoryBase(Container container)
    {
        _container = container;
    }

    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(id), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemQueryIterator<T>("SELECT * FROM c");
        var results = new List<T>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var response = await _container.CreateItemAsync(entity, new PartitionKey(entity.Id), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var response = await _container.UpsertItemAsync(entity, new PartitionKey(entity.Id), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public virtual async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<T>(id, new PartitionKey(id), cancellationToken: cancellationToken);
    }
}
