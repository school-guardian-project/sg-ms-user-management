namespace ms_user_management.Api.Driver.Application.Dto;

public class DriverLicenseRequestDto
{
    public Guid ProfileId { get; set; }
    public String LicenseNumber { get; set; } = string.Empty;
    public DateOnly LicenseExpirationDate { get; set; }
}
