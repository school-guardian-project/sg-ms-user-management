using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Driver.Application.Dto;

public class DriverLicenseResponseDto : DriverLicenseListDto
{
    public Status Status { get; set; }
}
