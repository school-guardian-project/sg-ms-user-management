using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Application.Mapper;
using ms_user_management.Api.Driver.Domain.Ports.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class GetDriverLicenseService
{
    private readonly IDriverLicenseRepository _repository;

    public GetDriverLicenseService(IDriverLicenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<DriverLicenseResponseDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var license = await _repository.GetByIdAsync(id, ct);

        if (license is null) return null;

        return DriverLicenseMapper.ToResponseDto(license);
    }
}
