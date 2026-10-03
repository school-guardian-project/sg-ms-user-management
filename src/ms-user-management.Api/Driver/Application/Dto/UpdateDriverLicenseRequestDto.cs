namespace ms_user_management.Api.Driver.Application.Dto;

public class UpdateDriverLicenseRequestDto
{
    public String LicenseNumber { get; set; } = string.Empty;
    public DateOnly LicenseExpirationDate { get; set; }
    public String? Status { get; set; }
}
