using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Auth;
using ms_user_management.Api.Shared.Infrastructure.Controller;
using Microsoft.AspNetCore.RateLimiting;

namespace ms_user_management.Api.Parent.Infrastructure.Controller;

[EnableRateLimiting("registration")]
[ApiController]
[Route("api/v1/parents")]
public class ParentController : ControllerBase
{
    private readonly ICreateParentUseCase _createParentService;
    private readonly IGetPersonUseCase _getParentService;
    private readonly IDeleteParentUseCase _deleteParentService;
    private readonly IListParentUseCase _listParentService;
    private readonly IUpdateParentUseCase _updateParentService;
    private readonly SearchPersonService _searchPersonService;

    public ParentController(ICreateParentUseCase createParentService, IGetPersonUseCase getParentService, IDeleteParentUseCase deleteParentService, IListParentUseCase listParentService, IUpdateParentUseCase updateParentService, SearchPersonService searchPersonService)
    {
        _createParentService = createParentService;
        _getParentService = getParentService;
        _deleteParentService = deleteParentService;
        _listParentService = listParentService;
        _updateParentService = updateParentService;
        _searchPersonService = searchPersonService;
    }
    
    [HttpPost]
    [RequirePermission(Permissions.StudentsManage)]
    public async Task<IActionResult> Create([FromBody] CreatePersonRequestDto dto)
    {
        try
        {
            await _createParentService.CreateAsync(dto);

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
            return Ok(await _getParentService.GetByIdAsync(id));
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _listParentService.ExecuteAsync());
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }
    
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.StudentsManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PersonRequestDto dto)
    {
        await _updateParentService.UpdateAsync(id, dto);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.StudentsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteParentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    [RequirePermission(Permissions.StudentsRead)]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var result = await _searchPersonService.SearchAsync(search, RoleId.Parent);

        return Ok(result);
    }
}
