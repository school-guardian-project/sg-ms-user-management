namespace ms_user_management.Api.Shared.Application.Dto;

public class PersonListDto
{
    public Guid Id { get; set; }
    public String Name { get; set; }
    public String LastName { get; set; }
    public String IdentificationNumber { get; set; }
    public int Phone { get; set; }
}