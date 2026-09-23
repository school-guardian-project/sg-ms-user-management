using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Student.Application.UseCase;

namespace ms_user_management.Api.Student.Infrastructure.Controller;

[ApiController]
[Route("api/students")]
public class StudentController : ControllerBase
{
    private readonly CreateStudentService _createStudentService;
    private readonly GetStudentService _getStudentService;
    private readonly DeleteStudentService _deleteStudentService;
    private readonly ListStudentService _listStudentService;
    private readonly UpdateStudentService _updateStudentService;
    private readonly SearchPersonService _searchPersonService;

    public StudentController(CreateStudentService createStudentService, GetStudentService getStudentService, DeleteStudentService deleteStudentService, ListStudentService listStudentService, UpdateStudentService updateStudentService, SearchPersonService searchPersonService)
    {
        _createStudentService = createStudentService;
        _getStudentService = getStudentService;
        _deleteStudentService = deleteStudentService;
        _listStudentService = listStudentService;
        _updateStudentService = updateStudentService;
        _searchPersonService = searchPersonService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PersonRequestDto dto)
    {
        await _createStudentService.CreateAsync(dto);

        return Ok();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getStudentService.GetByIdAsync(id);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _listStudentService.ExecuteAsync();

        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PersonRequestDto dto)
    {
        await _updateStudentService.UpdateAsync(id, dto);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteStudentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var result = await _searchPersonService.SearchAsync(search);

        return Ok(result);
    }
}