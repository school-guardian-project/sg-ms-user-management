namespace ms_user_management.Api.Family.Application.Dto;

public class FamilyListDto
{
    public Guid Id { get; set; }
    public String Name { get; set; }
    public Guid? ParentProfileId { get; set; }
}
