using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Admin.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Auth;
using ms_user_management.Api.Shared.Infrastructure.Controller;
using Microsoft.AspNetCore.RateLimiting;

namespace ms_user_management.Api.Admin.Infrastructure.Controller;

[EnableRateLimiting("registration")]
[ApiController]
[Route("api/v1/admins")]
public class AdminController : ControllerBase
{
    private readonly ICreateAdminUseCase _createAdminService;
    private readonly IGetPersonUseCase _getAdminService;
    private readonly IDeleteAdminUseCase _deleteAdminService;
    private readonly IListAdminUseCase _listAdminService;
    private readonly IUpdateAdminUseCase _updateAdminService;
    private readonly SearchPersonService _searchPersonService;

    public AdminController(ICreateAdminUseCase createAdminService, IGetPersonUseCase getAdminService, IDeleteAdminUseCase deleteAdminService, IListAdminUseCase listAdminService, IUpdateAdminUseCase updateAdminService, SearchPersonService searchPersonService)
    {
        _createAdminService = createAdminService;
        _getAdminService = getAdminService;
        _deleteAdminService = deleteAdminService;
        _listAdminService = listAdminService;
        _updateAdminService = updateAdminService;
        _searchPersonService = searchPersonService;
    }
    
    [HttpPost]
    [RequireRole(RequireRoleAttribute.SuperAdminRoleId)]
    public async Task<IActionResult> Create([FromBody] CreateAdminRequestDto dto)
    {
        try
        {
            await _createAdminService.CreateAsync(dto);

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
            return Ok(await _getAdminService.GetByIdAsync(id));
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }

    [HttpGet]
    [RequireRole(RequireRoleAttribute.SuperAdminRoleId)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _listAdminService.ExecuteAsync());
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }
    
    [HttpPut("{id:guid}")]
    [RequireRole(RequireRoleAttribute.SuperAdminRoleId)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PersonRequestDto dto)
    {
        await _updateAdminService.UpdateAsync(id, dto);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [RequireRole(RequireRoleAttribute.SuperAdminRoleId)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteAdminService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    [RequireRole(RequireRoleAttribute.SuperAdminRoleId)]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var result = await _searchPersonService.SearchAsync(search, RoleId.Admin);

        return Ok(result);
    }
}
