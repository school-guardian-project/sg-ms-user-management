namespace ms_user_management.Api.Family.Application.Dto;

public class RegisterFamilyRequest
{
    public string FamilyName { get; set; }
    public string? Observations { get; set; }
    public List<FamilyMemberRequest> Members { get; set; } = new();
}
