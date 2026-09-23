using AutoMapper;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;

namespace ms_user_management.Api.Shared.Application.Search;

public class SearchPersonService
{
    private readonly IEnumerable<IPersonSearchStrategy> _strategies;
    private readonly IMapper _mapper;

    public SearchPersonService(IEnumerable<IPersonSearchStrategy> strategies, IMapper mapper)
    {
        _strategies = strategies;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonListDto>> SearchAsync(string search)
    {
        search = search.Trim();

        if (string.IsNullOrEmpty(search)) return [];

        var strategy = _strategies.FirstOrDefault(x => x.CanHandle(search));

        if (strategy == null) return [];
        
        var persons = await strategy.SearchAsync(search);
        
        return _mapper.Map<IEnumerable<PersonListDto>>(persons);
    }
}