using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;

namespace ms_user_management.Api.Shared.Application.Search.Strategy;

public class IdentificationSearchStrategy : IPersonSearchStrategy
{
    private readonly IPersonSearchRepository _personSearchRepository;

    public IdentificationSearchStrategy(IPersonSearchRepository personSearchRepository)
    {
        _personSearchRepository = personSearchRepository;
    }

    public bool CanHandle(string search)
    {
        return search.All(char.IsDigit);
    }

    public Task<IEnumerable<Person>> SearchAsync(string search)
    {
        return _personSearchRepository.SearchByIdentificationAsync(search);
    }
}