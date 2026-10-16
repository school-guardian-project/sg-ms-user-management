using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Tenant;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Family.Application.UseCase;

public class ListFamilyService : IListFamilyUseCase
{
    private readonly IFamilyRepository _repository;
    private readonly TenantPersonFilter _tenantFilter;
    private readonly IPersonProfileReader _profileReader;

    public ListFamilyService(
        IFamilyRepository familyRepository,
        TenantPersonFilter tenantFilter,
        IPersonProfileReader profileReader)
    {
        _repository = familyRepository;
        _tenantFilter = tenantFilter;
        _profileReader = profileReader;
    }

    public async Task<IEnumerable<FamilyListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var families = await _repository.GetAllAsync(ct);

        // Multi-tenant: si hay filtro, solo familias cuyo acudiente principal
        // pertenece a una sede visible. Sin filtro (interno/SuperAdmin), todas.
        var visibleCampusIds = await _tenantFilter.GetVisibleCampusIdsAsync(ct);
        IReadOnlySet<Guid>? visibleProfiles = visibleCampusIds is null
            ? null
            : await _profileReader.GetProfileIdsInCampusesAsync(visibleCampusIds, ct);

        return families
            .Where(f => visibleProfiles is null
                || (f.ParentProfileId is { } parentId && visibleProfiles.Contains(parentId)))
            .Select(f => new FamilyListDto
            {
                Id = f.Family.Id,
                Name = f.Family.Name,
                ParentProfileId = f.ParentProfileId
            }).ToList();
    }
}
