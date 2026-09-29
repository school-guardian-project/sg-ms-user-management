using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Family.Domain.Model;

public class FamilyMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FamilyId { get; set; }
    public Guid ProfileId { get; set; }
    public Status Status { get; set; }
    public RelationType RelationType { get; set; }
    public Family Family { get; set; }
}