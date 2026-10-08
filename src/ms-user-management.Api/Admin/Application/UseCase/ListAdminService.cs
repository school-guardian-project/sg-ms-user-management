using AutoMapper;
using ms_user_management.Api.Admin.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Tenant;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Admin.Application.UseCase;

public class ListAdminService : IListAdminUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly TenantPersonFilter _tenantFilter;
    private readonly IMapper _mapper;

    public ListAdminService(
        IPersonRepository personRepository,
        TenantPersonFilter tenantFilter,
        IMapper mapper)
    {
        _personRepository = personRepository;
        _tenantFilter = tenantFilter;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonListDto>> ExecuteAsync()
    {
        var admins = await _tenantFilter.GetVisiblePersonIdsAsync(RoleId.Admin);
        var persons = (await _personRepository.GetAllAsync())
            .Where(p => admins.Contains(p.Id));

        return _mapper.Map<IEnumerable<PersonListDto>>(persons);
    }
}
