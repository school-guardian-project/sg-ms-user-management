using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.Out;
using ms_user_management.Api.Family.Infrastructure.Persistence;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;
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

    public async Task<IReadOnlyList<(FamilyModel Family, Guid? ParentProfileId)>> GetAllAsync(
        CancellationToken ct = default)
    {
        var families = await _userManagementContext.Families
            .AsNoTracking()
            .ToListAsync(ct);

        var familyIds = families.Select(f => f.Id).ToList();
        var parents = await _userManagementContext.FamilyMembers
            .AsNoTracking()
            .Where(fm => familyIds.Contains(fm.FamilyId) && fm.RelationType == RelationType.Parent)
            .Select(fm => new { fm.FamilyId, fm.ProfileId })
            .ToListAsync(ct);

        var parentByFamily = parents
            .GroupBy(p => p.FamilyId)
            .ToDictionary(g => g.Key, g => (Guid?)g.First().ProfileId);

        return families
            .Select(f => (ToDomain(f), parentByFamily.GetValueOrDefault(f.Id)))
            .ToList();
    }

    public async Task<FamilyModel?> GetByIdAsync(Guid familyId, CancellationToken ct = default)
    {
        var family = await _userManagementContext.Families
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == familyId, ct);

        return family is null ? null : ToDomain(family);
    }

    public async Task<IReadOnlyList<(Guid ProfileId, RelationType RelationType)>> GetMembersAsync(
        Guid familyId, CancellationToken ct = default)
    {
        var members = await _userManagementContext.FamilyMembers
            .AsNoTracking()
            .Where(fm => fm.FamilyId == familyId)
            .ToListAsync(ct);

        return members.Select(m => (m.ProfileId, m.RelationType)).ToList();
    }

    public async Task UpdateAsync(Guid familyId, string familyName, string observations,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default)
    {
        var strategy = _userManagementContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _userManagementContext.Database.BeginTransactionAsync(ct);

            var family = await _userManagementContext.Families
                .FirstOrDefaultAsync(f => f.Id == familyId, ct);
            if (family is null)
                throw new InvalidOperationException($"Family not found: {familyId}");

            family.Name = familyName;
            family.Observations = observations;

            var currentMembers = await _userManagementContext.FamilyMembers
                .Where(fm => fm.FamilyId == familyId)
                .ToListAsync(ct);
            _userManagementContext.FamilyMembers.RemoveRange(currentMembers);

            _userManagementContext.FamilyMembers.AddRange(
                members.Select(m => new FamilyMemberEntity
                {
                    Id = Guid.NewGuid(),
                    FamilyId = familyId,
                    ProfileId = m.ProfileId,
                    RelationType = m.RelationType,
                    Status = Status.Active
                }));

            await _userManagementContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
    }

    public async Task DeleteAsync(Guid familyId, CancellationToken ct = default)
    {
        var family = await _userManagementContext.Families
            .FirstOrDefaultAsync(f => f.Id == familyId, ct);
        if (family is null)
            throw new InvalidOperationException($"Family not found: {familyId}");

        await using var tx = await _userManagementContext.Database.BeginTransactionAsync(ct);
        var members = await _userManagementContext.FamilyMembers
            .Where(fm => fm.FamilyId == familyId)
            .ToListAsync(ct);
        _userManagementContext.FamilyMembers.RemoveRange(members);
        await _userManagementContext.SaveChangesAsync(ct);

        _userManagementContext.Families.Remove(family);
        await _userManagementContext.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<FamilySearchRow>> GetAllWithGuardianAsync(
        CancellationToken ct = default)
    {
        var families = await _userManagementContext.Families
            .AsNoTracking()
            .ToListAsync(ct);

        var familyIds = families.Select(f => f.Id).ToList();
        var parents = await _userManagementContext.FamilyMembers
            .AsNoTracking()
            .Where(fm => familyIds.Contains(fm.FamilyId) && fm.RelationType == RelationType.Parent)
            .Select(fm => new { fm.FamilyId, fm.ProfileId })
            .ToListAsync(ct);

        var profileIds = parents.Select(p => p.ProfileId).Distinct().ToList();
        var profiles = await _userManagementContext.Set<ProfileRefEntity>()
            .AsNoTracking()
            .Where(p => profileIds.Contains(p.Id))
            .Select(p => new { p.Id, p.PersonId })
            .ToListAsync(ct);

        var personIds = profiles.Select(p => p.PersonId).Distinct().ToList();
        var persons = await _userManagementContext.Person
            .AsNoTracking()
            .Where(p => personIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.LastName, p.Phone })
            .ToListAsync(ct);

        var parentByFamily = parents
            .GroupBy(p => p.FamilyId)
            .ToDictionary(g => g.Key, g => (Guid?)g.First().ProfileId);
        var personByProfile = profiles.ToDictionary(p => p.Id, p => p.PersonId);
        var personById = persons.ToDictionary(p => p.Id, p => p);

        return families.Select(f =>
        {
            Guid? parentProfileId = parentByFamily.GetValueOrDefault(f.Id);
            var guardian = parentProfileId != null
                && personByProfile.TryGetValue(parentProfileId.Value, out var personId)
                && personById.TryGetValue(personId, out var person)
                ? person
                : null;

            return new FamilySearchRow
            {
                FamilyId = f.Id,
                FamilyName = f.Name,
                Observations = f.Observations,
                ParentProfileId = parentProfileId,
                GuardianName = guardian?.Name ?? string.Empty,
                GuardianLastName = guardian?.LastName ?? string.Empty,
                GuardianPhone = guardian?.Phone ?? 0
            };
        }).ToList();
    }

    private static FamilyModel ToDomain(FamilyEntity family) => new()
    {
        Id = family.Id,
        Name = family.Name,
        Observations = family.Observations,
        Status = family.Status
    };
}
