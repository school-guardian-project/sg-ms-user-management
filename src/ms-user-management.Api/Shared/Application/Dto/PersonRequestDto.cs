namespace ms_user_management.Api.Shared.Application.Dto;

public class PersonRequestDto
{
    public String Name { get; set; }
    public String LastName { get; set; }
    public string IdentificationType { get; set; }
    public String IdentificationNumber { get; set; }
    public String Email { get; set; }
    public int Phone { get; set; }
    public String ResidenceAddress { get; set; }
    public DateOnly DateBirth { get; set; }
}