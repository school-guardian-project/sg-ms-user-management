using ms_user_management.Api.Family.Domain.Model;

namespace ms_user_management.Api.Family.Domain.Ports.Out;

public interface IFamilySearchStrategy
{
    bool CanHandle(string search);

    IEnumerable<FamilySearchRow> Search(IEnumerable<FamilySearchRow> items, string search);
}
