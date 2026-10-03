using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Driver.Infrastructure.Persistence;

public class DriverLicenseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfileId { get; set; }
    public String LicenseNumber { get; set; } = string.Empty;
    public DateOnly LicenseExpirationDate { get; set; }
    public Status Status { get; set; }
}
