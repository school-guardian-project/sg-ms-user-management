namespace ms_user_management.Api.Shared.Domain.Port.Out;

public interface IPersonProfileReader
{
    Task<IReadOnlyDictionary<Guid, Guid>> GetProfileIdsByPersonIdsAsync(
        IReadOnlyCollection<Guid> personIds,
        CancellationToken ct = default);

    Task<bool> ProfileExistsAsync(Guid profileId, CancellationToken ct = default);

    Task<string?> GetPersonNameAsync(Guid profileId, CancellationToken ct = default);
}
