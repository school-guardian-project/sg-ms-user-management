using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Auth;
using ms_user_management.Api.Shared.Infrastructure.Controller;
using ms_user_management.Api.Student.Domain.Ports.In;
using Microsoft.AspNetCore.RateLimiting;

namespace ms_user_management.Api.Student.Infrastructure.Controller;

[EnableRateLimiting("registration")]
[ApiController]
[Route("api/v1/students")]
public class StudentController : ControllerBase
{
    private readonly ICreateStudentUseCase _createStudentService;
    private readonly IGetPersonUseCase _getStudentService;
    private readonly IDeleteStudentUseCase _deleteStudentService;
    private readonly IListStudentUseCase _listStudentService;
    private readonly IUpdateStudentUseCase _updateStudentService;
    private readonly SearchPersonService _searchPersonService;

    public StudentController(ICreateStudentUseCase createStudentService, IGetPersonUseCase getStudentService, IDeleteStudentUseCase deleteStudentService, IListStudentUseCase listStudentService, IUpdateStudentUseCase updateStudentService, SearchPersonService searchPersonService)
    {
        _createStudentService = createStudentService;
        _getStudentService = getStudentService;
        _deleteStudentService = deleteStudentService;
        _listStudentService = listStudentService;
        _updateStudentService = updateStudentService;
        _searchPersonService = searchPersonService;
    }
    
    [HttpPost]
    [RequirePermission(Permissions.StudentsManage)]
    public async Task<IActionResult> Create([FromBody] CreatePersonRequestDto dto)
    {
        try
        {
            await _createStudentService.CreateAsync(dto);

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
            return Ok(await _getStudentService.GetByIdAsync(id));
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
            return Ok(await _listStudentService.ExecuteAsync());
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
        await _updateStudentService.UpdateAsync(id, dto);
        
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.StudentsManage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteStudentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    [RequirePermission(Permissions.StudentsRead)]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var result = await _searchPersonService.SearchAsync(search, RoleId.Student);

        return Ok(result);
    }
}