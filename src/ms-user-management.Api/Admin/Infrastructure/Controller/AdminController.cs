using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Admin.Application.UseCase;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Search;

namespace ms_user_management.Api.Admin.Infrastructure.Controller;

[ApiController]
[Route("api/admins")]
public class AdminController : ControllerBase
{
    private readonly CreateAdminService _createAdminService;
    private readonly GetAdminService _getAdminService;
    private readonly DeleteAdminService _deleteAdminService;
    private readonly ListAdminService _listAdminService;
    private readonly UpdateAdminService _updateAdminService;
    private readonly SearchPersonService _searchPersonService;

    public AdminController(CreateAdminService createAdminService, GetAdminService getAdminService, DeleteAdminService deleteAdminService, ListAdminService listAdminService, UpdateAdminService updateAdminService, SearchPersonService searchPersonService)
    {
        _createAdminService = createAdminService;
        _getAdminService = getAdminService;
        _deleteAdminService = deleteAdminService;
        _listAdminService = listAdminService;
        _updateAdminService = updateAdminService;
        _searchPersonService = searchPersonService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PersonRequestDto dto)
    {
        await _createAdminService.CreateAsync(dto);

        return Ok();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getAdminService.GetByIdAsync(id);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listAdminService.ExecuteAsync();

        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PersonRequestDto dto)
    {
        await _updateAdminService.UpdateAsync(id, dto);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteAdminService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var result = await _searchPersonService.SearchAsync(search);

        return Ok(result);
    }
}
