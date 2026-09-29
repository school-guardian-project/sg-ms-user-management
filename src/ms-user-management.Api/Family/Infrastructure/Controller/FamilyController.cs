using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Infrastructure.Controller.Mapper;

namespace ms_user_management.Api.Family.Infrastructure.Controller;

[ApiController]
[Route("api/families")]
public class FamilyController : ControllerBase
{
    private readonly IRegisterFamilyUseCase _useCase;

    public FamilyController(IRegisterFamilyUseCase useCase) => _useCase = useCase;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterFamilyRequest request, CancellationToken ct)
    {
        var (familyId, members) = await _useCase.ExecuteAsync(
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
