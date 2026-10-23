namespace ms_user_management.Api.Family.Application.Dto;

/// <summary>
/// Confirmación de registro de familia con sus miembros. Sustituye al objeto
/// anónimo que se devolvía antes en el POST; el JSON serializado es idéntico
/// (<c>{"familyId": ..., "members": [...]}</c>).
/// </summary>
public record RegisterFamilyResponseDto(
    Guid FamilyId,
    IEnumerable<FamilyMemberResponseDto> Members);
