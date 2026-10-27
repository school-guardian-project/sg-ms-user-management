using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Auth;
using ms_user_management.Api.Shared.Infrastructure.Controller;
using Microsoft.AspNetCore.RateLimiting;

namespace ms_user_management.Api.Driver.Infrastructure.Controller;

[EnableRateLimiting("registration")]
[ApiController]
[Route("api/v1/drivers")]
public class DriverController : ControllerBase
{
    private readonly ICreateDriverUseCase _createDriverService;
    private readonly IGetPersonUseCase _getDriverService;
    private readonly IDeleteDriverUseCase _deleteDriverService;
    private readonly IListDriverUseCase _listDriverService;
    private readonly IUpdateDriverUseCase _updateDriverService;
    private readonly SearchPersonService _searchPersonService;

    public DriverController(ICreateDriverUseCase createDriverService, IGetPersonUseCase getDriverService, IDeleteDriverUseCase deleteDriverService, IListDriverUseCase listDriverService, IUpdateDriverUseCase updateDriverService, SearchPersonService searchPersonService)
    {
        _createDriverService = createDriverService;
        _getDriverService = getDriverService;
        _deleteDriverService = deleteDriverService;
        _listDriverService = listDriverService;
        _updateDriverService = updateDriverService;
        _searchPersonService = searchPersonService;
    }
    
    [HttpPost]
    [RequirePermission(Permissions.DriversManage)]
    public async Task<IActionResult> Create([FromBody] CreatePersonRequestDto dto)
    {
        try
        {
            await _createDriverService.CreateAsync(dto);

            return Ok();
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }

    // Propio, familia o staff del colegio: lo decide el guard de aplicacion.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            return Ok(await _getDriverService.GetByIdAsync(id));
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }

    [HttpGet]
    [RequirePermission(Permissions.DriversRead)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _listDriverService.ExecuteAsync());
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }
    
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.DriversManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PersonRequestDto dto)
    {
        await _updateDriverService.UpdateAsync(id, dto);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.DriversManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteDriverService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    [RequirePermission(Permissions.DriversRead)]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var result = await _searchPersonService.SearchAsync(search, RoleId.Driver);

        return Ok(result);
    }
}
