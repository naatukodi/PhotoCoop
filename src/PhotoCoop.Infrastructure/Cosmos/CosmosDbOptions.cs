namespace PhotoCoop.Infrastructure.Cosmos;

public class CosmosDbOptions
{
    public const string SectionName = "CosmosDb";

    public string AccountEndpoint { get; set; } = null!;
    public string AccountKey { get; set; } = null!;
    public string DatabaseId { get; set; } = "photo-coop-db";

    public string UsersContainerId { get; set; } = "Users";
    public string BookingsContainerId { get; set; } = "Bookings";
}
