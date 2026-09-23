using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Search;

namespace ms_user_management.Api.Driver.Infrastructure.Controller;

[ApiController]
[Route("api/drivers")]
public class DriverController : ControllerBase
{
    private readonly CreateDriverService _createDriverService;
    private readonly GetDriverService _getDriverService;
    private readonly DeleteDriverService _deleteDriverService;
    private readonly ListDriverService _listDriverService;
    private readonly UpdateDriverService _updateDriverService;
    private readonly SearchPersonService _searchPersonService;

    public DriverController(CreateDriverService createDriverService, GetDriverService getDriverService, DeleteDriverService deleteDriverService, ListDriverService listDriverService, UpdateDriverService updateDriverService, SearchPersonService searchPersonService)
    {
        _createDriverService = createDriverService;
        _getDriverService = getDriverService;
        _deleteDriverService = deleteDriverService;
        _listDriverService = listDriverService;
        _updateDriverService = updateDriverService;
        _searchPersonService = searchPersonService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PersonRequestDto dto)
    {
        await _createDriverService.CreateAsync(dto);

        return Ok();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getDriverService.GetByIdAsync(id);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listDriverService.ExecuteAsync();

        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PersonRequestDto dto)
    {
        await _updateDriverService.UpdateAsync(id, dto);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteDriverService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var result = await _searchPersonService.SearchAsync(search);

        return Ok(result);
    }
}
