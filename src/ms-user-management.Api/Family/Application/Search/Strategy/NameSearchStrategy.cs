using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.Out;

namespace ms_user_management.Api.Family.Application.Search.Strategy;

public class NameSearchStrategy : IFamilySearchStrategy
{
    public bool CanHandle(string search) => true;

    public IEnumerable<FamilySearchRow> Search(IEnumerable<FamilySearchRow> items, string search)
        => items.Where(row => Contains(row.FamilyName, search)
            || Contains(row.Observations, search)
            || Contains(row.GuardianName, search)
            || Contains(row.GuardianLastName, search));

    private static bool Contains(string? value, string search)
        => !string.IsNullOrEmpty(value) && value.Contains(search, StringComparison.OrdinalIgnoreCase);
}
