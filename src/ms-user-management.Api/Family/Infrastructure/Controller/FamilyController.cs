using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Infrastructure.Mapper;

namespace ms_user_management.Api.Family.Infrastructure.Controller;

[ApiController]
[Route("api/families")]
public class FamilyController : ControllerBase
{
    private readonly IRegisterFamilyUseCase _registerUseCase;
    private readonly IListFamilyUseCase _listUseCase;
    private readonly IGetFamilyUseCase _getUseCase;
    private readonly IUpdateFamilyUseCase _updateUseCase;
    private readonly IDeleteFamilyUseCase _deleteUseCase;
    private readonly GetFamilyMembersByStudentService _membersByStudentService;
    private readonly SearchFamilyService _searchFamilyService;

    public FamilyController(IRegisterFamilyUseCase registerUseCase,
        IListFamilyUseCase listUseCase,
        IGetFamilyUseCase getUseCase,
        IUpdateFamilyUseCase updateUseCase,
        IDeleteFamilyUseCase deleteUseCase,
        GetFamilyMembersByStudentService membersByStudentService,
        SearchFamilyService searchFamilyService)
    {
        _registerUseCase = registerUseCase;
        _listUseCase = listUseCase;
        _getUseCase = getUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
        _membersByStudentService = membersByStudentService;
        _searchFamilyService = searchFamilyService;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterFamilyRequest request, CancellationToken ct)
    {
        var (familyId, members) = await _registerUseCase.ExecuteAsync(
            request.FamilyName,
            request.Observations,
            FamilyRequestMapper.ToUseCaseInput(request.Members),
            ct);

        return Ok(new
        {
            familyId,
            members = members.Select(m => new
            {
                memberId = m.MemberId,
                profileId = m.ProfileId
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _listUseCase.ExecuteAsync(ct);

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search, CancellationToken ct)
    {
        var result = await _searchFamilyService.SearchAsync(search, ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getUseCase.ExecuteAsync(id, ct);

        return Ok(result);
    }

    [HttpGet("student/{studentProfileId:guid}/members")]
    public async Task<IActionResult> GetMembersByStudent(Guid studentProfileId, CancellationToken ct)
    {
        var parentProfileIds = await _membersByStudentService.ExecuteAsync(studentProfileId, ct);

        return Ok(parentProfileIds.Select(id => new { profileId = id }));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RegisterFamilyRequest request, CancellationToken ct)
    {
        await _updateUseCase.ExecuteAsync(
            id,
            request.FamilyName,
            request.Observations,
            FamilyRequestMapper.ToUseCaseInput(request.Members),
            ct);

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _deleteUseCase.ExecuteAsync(id, ct);

        return NoContent();
    }

    public class RegisterFamilyRequest
    {
        public string FamilyName { get; set; }
        public string? Observations { get; set; }
        public List<FamilyMemberRequest> Members { get; set; } = new();
    }

    public class FamilyMemberRequest
    {
        public string ProfileId { get; set; }
        public string RelationshipType { get; set; }
    }
}
