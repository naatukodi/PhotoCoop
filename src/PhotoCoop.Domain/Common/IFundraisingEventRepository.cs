namespace PhotoCoop.Domain.Fundraising;

public interface IFundraisingEventRepository : PhotoCoop.Domain.Common.IRepository<FundraisingEvent>
{
    Task<IReadOnlyList<FundraisingEvent>> GetActiveEventsAsync(CancellationToken cancellationToken = default);
}
