using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Application.Dto;

public class PersonResponseDto : PersonListDto
{
    public IdentificationType IdentificationType { get; set; }
    public String Email { get; set; }
    public String ResidenceAddress { get; set; }
    public DateOnly DateBirth { get; set; }
}