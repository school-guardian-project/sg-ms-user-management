using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Parent.Application.UseCase;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Search;

namespace ms_user_management.Api.Parent.Infrastructure.Controller;

[ApiController]
[Route("api/parents")]
public class ParentController : ControllerBase
{
    private readonly CreateParentService _createParentService;
    private readonly GetParentService _getParentService;
    private readonly DeleteParentService _deleteParentService;
    private readonly ListParentService _listParentService;
    private readonly UpdateParentService _updateParentService;
    private readonly SearchPersonService _searchPersonService;

    public ParentController(CreateParentService createParentService, GetParentService getParentService, DeleteParentService deleteParentService, ListParentService listParentService, UpdateParentService updateParentService, SearchPersonService searchPersonService)
    {
        _createParentService = createParentService;
        _getParentService = getParentService;
        _deleteParentService = deleteParentService;
        _listParentService = listParentService;
        _updateParentService = updateParentService;
        _searchPersonService = searchPersonService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PersonRequestDto dto)
    {
        await _createParentService.CreateAsync(dto);

        return Ok();
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
    public async Task<IActionResult> Update(Guid id, [FromBody] PersonRequestDto dto)
    {
        await _updateParentService.UpdateAsync(id, dto);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteParentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var result = await _searchPersonService.SearchAsync(search);

        return Ok(result);
    }
}
