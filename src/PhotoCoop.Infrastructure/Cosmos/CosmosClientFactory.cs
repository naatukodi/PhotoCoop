using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace PhotoCoop.Infrastructure.Cosmos;

public class CosmosClientFactory
{
    public CosmosClient Client { get; }
    public Database Database { get; }

    public Container UsersContainer { get; }
    public Container BookingsContainer { get; }

    public CosmosClientFactory(IOptions<CosmosDbOptions> options)
    {
        var opt = options.Value;

        if (string.IsNullOrWhiteSpace(opt.AccountEndpoint) ||
            string.IsNullOrWhiteSpace(opt.AccountKey))
        {
            throw new InvalidOperationException(
                "CosmosDb configuration is missing AccountEndpoint or AccountKey. " +
                "Set CosmosDb:AccountEndpoint and CosmosDb:AccountKey (or environment variables) to start the API.");
        }

        Client = new CosmosClient(opt.AccountEndpoint, opt.AccountKey);

        Database = Client.CreateDatabaseIfNotExistsAsync(opt.DatabaseId).GetAwaiter().GetResult();

        UsersContainer = Database.CreateContainerIfNotExistsAsync(
            opt.UsersContainerId,
            partitionKeyPath: "/id"   // simple: partition by id
        ).GetAwaiter().GetResult();

        BookingsContainer = Database.CreateContainerIfNotExistsAsync(
            opt.BookingsContainerId,
            partitionKeyPath: "/customerUserId" // bookings grouped by customer
        ).GetAwaiter().GetResult();
    }
}
