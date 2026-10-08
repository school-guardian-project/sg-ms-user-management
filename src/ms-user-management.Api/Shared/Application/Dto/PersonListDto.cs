namespace ms_user_management.Api.Shared.Application.Dto;

public class PersonListDto
{
    public Guid Id { get; set; }
    public String Name { get; set; }
    public String LastName { get; set; }
    public String IdentificationNumber { get; set; }
    public String Email { get; set; }
    public long Phone { get; set; }
    public Guid? ProfileId { get; set; }

    // Solo conductores: vienen de UserManagement.DriverLicense (null en los demás roles).
    public String? LicenseNumber { get; set; }
    public DateOnly? LicenseExpirationDate { get; set; }
}