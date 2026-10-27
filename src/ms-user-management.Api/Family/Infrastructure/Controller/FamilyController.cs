using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Infrastructure.Mapper;
using Microsoft.AspNetCore.RateLimiting;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Auth;
using ms_user_management.Api.Shared.Infrastructure.Controller;

namespace ms_user_management.Api.Family.Infrastructure.Controller;

[EnableRateLimiting("registration")]
[ApiController]
[Route("api/v1/families")]
public class FamilyController : ControllerBase
{
    private readonly IRegisterFamilyUseCase _registerUseCase;
    private readonly IListFamilyUseCase _listUseCase;
    private readonly IGetFamilyUseCase _getUseCase;
    private readonly IUpdateFamilyUseCase _updateUseCase;
    private readonly IDeleteFamilyUseCase _deleteUseCase;
    private readonly GetFamilyMembersByStudentService _membersByStudentService;
    private readonly SearchFamilyService _searchFamilyService;
    private readonly PersonAccessGuard _accessGuard;

    public FamilyController(IRegisterFamilyUseCase registerUseCase,
        IListFamilyUseCase listUseCase,
        IGetFamilyUseCase getUseCase,
        IUpdateFamilyUseCase updateUseCase,
        IDeleteFamilyUseCase deleteUseCase,
        GetFamilyMembersByStudentService membersByStudentService,
        SearchFamilyService searchFamilyService,
        PersonAccessGuard accessGuard)
    {
        _registerUseCase = registerUseCase;
        _listUseCase = listUseCase;
        _getUseCase = getUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
        _membersByStudentService = membersByStudentService;
        _searchFamilyService = searchFamilyService;
        _accessGuard = accessGuard;
    }

    [HttpPost]
    [RequirePermission(Permissions.StudentsManage)]
    public async Task<IActionResult> Register([FromBody] RegisterFamilyRequest request, CancellationToken ct)
    {
        try
        {
            var (familyId, members) = await _registerUseCase.ExecuteAsync(
                request.FamilyName,
                request.Observations,
                FamilyRequestMapper.ToUseCaseInput(request.Members),
                ct);

            return Ok(new RegisterFamilyResponseDto(
                familyId,
                members.Select(m => new FamilyMemberResponseDto(m.MemberId, m.ProfileId))));
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }

    // Staff: las de su colegio. Acudiente / estudiante: solo las suyas.
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        try
        {
            return Ok(await _listUseCase.ExecuteAsync(ct));
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }

    [HttpGet("search")]
    [RequirePermission(Permissions.StudentsRead)]
    public async Task<IActionResult> Search([FromQuery] string search, CancellationToken ct)
    {
        var result = await _searchFamilyService.SearchAsync(search, ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            return Ok(await _getUseCase.ExecuteAsync(id, ct));
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }

    [HttpGet("student/{studentProfileId:guid}/members")]
    public async Task<IActionResult> GetMembersByStudent(Guid studentProfileId, CancellationToken ct)
    {
        try
        {
            // El guard va aqui y no en el servicio: el gRPC interno
            // (FamilyGrpcService) reutiliza el servicio sin usuario.
            await _accessGuard.EnsureCanReadStudentFamilyAsync(studentProfileId, ct);
            var parentProfileIds = await _membersByStudentService.ExecuteAsync(studentProfileId, ct);

            return Ok(parentProfileIds.Select(id => new FamilyMemberProfileDto(id)));
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.StudentsManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] RegisterFamilyRequest request, CancellationToken ct)
    {
        try
        {
            await _updateUseCase.ExecuteAsync(
                id,
                request.FamilyName,
                request.Observations,
                FamilyRequestMapper.ToUseCaseInput(request.Members),
                ct);

            return Ok();
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.StudentsManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _deleteUseCase.ExecuteAsync(id, ct);

        return NoContent();
    }
}
