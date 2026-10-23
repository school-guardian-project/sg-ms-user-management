using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Controller;
using ms_user_management.Api.Shared.Infrastructure.Auth;
using Microsoft.AspNetCore.RateLimiting;

namespace ms_user_management.Api.Parent.Infrastructure.Controller;

[EnableRateLimiting("registration")]
[ApiController]
[Route("api/v1/parents")]
[RequireProfile]
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
    [RequirePermission("users.manage")]
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getParentService.GetByIdAsync(id);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listParentService.ExecuteAsync();

        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PersonRequestDto dto)
    {
        await _updateParentService.UpdateAsync(id, dto);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteParentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var result = await _searchPersonService.SearchAsync(search, RoleId.Parent);

        return Ok(result);
    }
}
