namespace ms_user_management.Api.Family.Application.Dto;

public class FamilyResponseDto : FamilyListDto
{
    public String Description { get; set; }
    public List<Guid> Children { get; set; } = new();
}
