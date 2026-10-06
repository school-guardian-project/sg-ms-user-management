using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Infrastructure.Controller;

namespace ms_user_management.Api.Family.Infrastructure.Mapper;

public static class FamilyRequestMapper
{
    public static IReadOnlyList<(Guid ProfileId, RelationType RelationType)> ToUseCaseInput(
        IEnumerable<FamilyController.FamilyMemberRequest> members) =>
        members.Select(m => (
            ProfileId: Guid.Parse(m.ProfileId),
            RelationType: ParseRelationship(m.RelationshipType)
        )).ToList();

    private static RelationType ParseRelationship(string value)
    {
        if (!Enum.TryParse<RelationType>(value, true, out var r) || !Enum.IsDefined(r))
            throw new ArgumentException($"Invalid relationshipType: {value}. Use Parent or Student.");
        return r;
    }
}
