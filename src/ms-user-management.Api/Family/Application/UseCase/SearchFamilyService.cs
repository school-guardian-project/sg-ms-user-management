using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Domain.Ports.Out;

namespace ms_user_management.Api.Family.Application.UseCase;

public class SearchFamilyService
{
    private readonly IFamilyRepository _repository;
    private readonly IEnumerable<IFamilySearchStrategy> _strategies;

    public SearchFamilyService(IFamilyRepository repository, IEnumerable<IFamilySearchStrategy> strategies)
    {
        _repository = repository;
        _strategies = strategies;
    }

    public async Task<IEnumerable<FamilyListDto>> SearchAsync(string search, CancellationToken ct = default)
    {
        search = search?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(search)) return [];

        var strategy = _strategies.FirstOrDefault(x => x.CanHandle(search));

        if (strategy == null) return [];

        var rows = await _repository.GetAllWithGuardianAsync(ct);

        return strategy.Search(rows, search)
            .Select(row => new FamilyListDto
            {
                Id = row.FamilyId,
                Name = row.FamilyName,
                ParentProfileId = row.ParentProfileId
            })
            .ToList();
    }
}
