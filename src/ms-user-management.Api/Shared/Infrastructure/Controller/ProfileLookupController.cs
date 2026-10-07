using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Infrastructure.Controller;

[ApiController]
[Route("api/profiles")]
public class ProfileLookupController : ControllerBase
{
    private readonly IPersonProfileReader _profileReader;

    public ProfileLookupController(IPersonProfileReader profileReader)
    {
        _profileReader = profileReader;
    }

    [HttpGet("{id:guid}/exists")]
    public async Task<IActionResult> Exists(Guid id, CancellationToken ct)
    {
        return await _profileReader.ProfileExistsAsync(id, ct) ? Ok() : NotFound();
    }

    [HttpGet("{id:guid}/name")]
    public async Task<IActionResult> Name(Guid id, CancellationToken ct)
    {
        var name = await _profileReader.GetPersonNameAsync(id, ct);
        return name is null ? NotFound() : Content(name, "text/plain");
    }
}
