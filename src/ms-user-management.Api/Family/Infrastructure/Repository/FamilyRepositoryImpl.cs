using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.Out;
using ms_user_management.Api.Family.Infrastructure.Persistence;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Api.Family.Infrastructure.Repository;

public class FamilyRepositoryImpl : IFamilyRepository
{
    private readonly UserManagementContext _userManagementContext;

    public FamilyRepositoryImpl(UserManagementContext userManagementContext)
    {
        _userManagementContext = userManagementContext;
    }

    public async Task<IReadOnlyList<Guid>> SaveAsync(FamilyModel family,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default)
    {
        var memberIds = members.Select(_ => Guid.NewGuid()).ToList();

        var strategy = _userManagementContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _userManagementContext.Database.BeginTransactionAsync(ct);

            _userManagementContext.Families.Add(new FamilyEntity
            {
                Id = family.Id,
                Name = family.Name,
                Observations = family.Observations,
                Status = family.Status
            });

            _userManagementContext.FamilyMembers.AddRange(
                members.Zip(memberIds, (m, id) => new FamilyMemberEntity
                {
                    Id = id,
                    FamilyId = family.Id,
                    ProfileId = m.ProfileId,
                    RelationType = m.RelationType,
                    Status = Status.Active
                }));

            await _userManagementContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        return memberIds;
    }

    public async Task<IReadOnlyList<Guid>> GetAssociatedProfileIdsAsync(
        IEnumerable<Guid> profileIds, CancellationToken ct = default)
    {
        var ids = profileIds.ToList();
        return await _userManagementContext.FamilyMembers
            .Where(fm => ids.Contains(fm.ProfileId))
            .Select(fm => fm.ProfileId)
            .Distinct()
            .ToListAsync(ct);
    }
}
