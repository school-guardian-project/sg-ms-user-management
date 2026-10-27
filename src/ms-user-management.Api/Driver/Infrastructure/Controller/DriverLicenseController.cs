using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Driver.Domain.Ports.In;
using Microsoft.AspNetCore.RateLimiting;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Auth;

namespace ms_user_management.Api.Driver.Infrastructure.Controller;

[EnableRateLimiting("registration")]
[ApiController]
[Route("api/v1/driver-licenses")]
public class DriverLicenseController : ControllerBase
{
    private readonly CreateDriverLicenseService _createService;
    private readonly GetDriverLicenseService _getService;
    private readonly ListDriverLicenseService _listService;
    private readonly IUpdateDriverLicenseUseCase _updateService;
    private readonly IDeleteDriverLicenseUseCase _deleteService;

    public DriverLicenseController(
        CreateDriverLicenseService createService,
        GetDriverLicenseService getService,
        ListDriverLicenseService listService,
        IUpdateDriverLicenseUseCase updateService,
        IDeleteDriverLicenseUseCase deleteService)
    {
        _createService = createService;
        _getService = getService;
        _listService = listService;
        _updateService = updateService;
        _deleteService = deleteService;
    }

    [HttpPost]
    [RequirePermission(Permissions.DriversManage)]
    public async Task<IActionResult> Create([FromBody] DriverLicenseRequestDto dto, CancellationToken ct)
    {
        var id = await _createService.ExecuteAsync(dto, ct);

        return Ok(new CreateDriverLicenseResponseDto(id, dto.ProfileId));
    }

    [HttpGet]
    [RequirePermission(Permissions.DriversRead)]
    public async Task<IActionResult> List([FromQuery] Guid? profileId, CancellationToken ct)
    {
        var result = await _listService.ExecuteAsync(profileId, ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.DriversRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getService.ExecuteAsync(id, ct);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.DriversManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverLicenseRequestDto dto, CancellationToken ct)
    {
        var updated = await _updateService.ExecuteAsync(id, dto, ct);

        return updated ? Ok() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.DriversManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _deleteService.ExecuteAsync(id, ct);

        return deleted ? NoContent() : NotFound();
    }
}
