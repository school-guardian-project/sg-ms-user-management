using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.Out;

namespace ms_user_management.Api.Family.Application.Search.Strategy;

public class PhoneSearchStrategy : IFamilySearchStrategy
{
    public bool CanHandle(string search)
        => search.Length >= 3 && search.All(char.IsDigit);

    public IEnumerable<FamilySearchRow> Search(IEnumerable<FamilySearchRow> items, string search)
        => items.Where(row => row.GuardianPhone != 0
            && row.GuardianPhone.ToString().Contains(search, StringComparison.Ordinal));
}
