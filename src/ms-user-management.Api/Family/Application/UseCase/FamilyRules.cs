using ms_user_management.Api.Family.Domain.Model;

namespace ms_user_management.Api.Family.Application.UseCase;

internal static class FamilyRules
{
    public static IReadOnlyList<(Guid ProfileId, RelationType RelationType)> Validate(
        string familyName,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)>? members)
    {
        if (string.IsNullOrWhiteSpace(familyName))
            throw new ArgumentException("FamilyName is required.", nameof(familyName));
        if (members is null || members.Count == 0)
            throw new ArgumentException("At least one member is required.", nameof(members));

        var relations = members.Select(m =>
        {
            if (m.ProfileId == Guid.Empty)
                throw new ArgumentException("ProfileId is required for every member.", nameof(members));
            if (!Enum.IsDefined(m.RelationType))
                throw new ArgumentException($"Invalid relationship: {(int)m.RelationType}. Use Parent or Student.");
            return m;
        }).ToList();

        var duplicated = relations.GroupBy(x => x.ProfileId)
            .Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicated.Count > 0)
            throw new InvalidOperationException(
                $"Duplicated profiles in request: {string.Join(", ", duplicated)}");

        return relations;
    }
}
