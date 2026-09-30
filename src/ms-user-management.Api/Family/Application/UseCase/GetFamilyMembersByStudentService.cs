using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.Out;

namespace ms_user_management.Api.Family.Application.UseCase;

public class GetFamilyMembersByStudentService
{
    private readonly IFamilyRepository _repository;

    public GetFamilyMembersByStudentService(IFamilyRepository familyRepository)
    {
        _repository = familyRepository;
    }

    public async Task<List<Guid>> ExecuteAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var families = await _repository.GetAllAsync(ct);

        foreach (var (family, _) in families)
        {
            var members = await _repository.GetMembersAsync(family.Id, ct);

            if (members.Any(m => m.ProfileId == studentProfileId))
            {
                return members
                    .Where(m => m.RelationType == RelationType.Parent)
                    .Select(m => m.ProfileId)
                    .ToList();
            }
        }

        return new List<Guid>();
    }
}
