using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Shared.Application.UseCase;
using ms_user_management.Api.Shared.Infrastructure.Controller;

namespace ms_user_management.Api.Shared.Infrastructure.Controller;

/// <summary>
/// Operaciones transversales sobre una persona, independientes de su rol. Hoy
/// solo el cambio de estado (activar / desactivar cuenta), que es comun a
/// student, driver, parent y admin.
/// </summary>
[ApiController]
[Route("api/v1/persons")]
public class PersonsController : ControllerBase
{
    private readonly ChangePersonStatusService _changeStatusService;

    public PersonsController(ChangePersonStatusService changeStatusService)
    {
        _changeStatusService = changeStatusService;
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangePersonStatusRequestDto dto)
    {
        try
        {
            await _changeStatusService.ChangeStatusAsync(id, dto.Status);
            return Ok();
        }
        catch (Exception ex)
        {
            return ApiErrors.ToProblem(ex);
        }
    }
}

/// <summary>
/// Estado nuevo de la cuenta. Se recibe como cadena (<c>Active</c> /
/// <c>Inactive</c>) para alinear el cuerpo HTTP con el evento publicado y no
/// obligar al frontend a conocer el ordinal del enum.
/// </summary>
public class ChangePersonStatusRequestDto
{
    public string Status { get; set; }
}
