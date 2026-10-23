namespace ms_user_management.Api.Driver.Application.Dto;

/// <summary>
/// Confirmación de alta de licencia. Sustituye al objeto anónimo que se
/// devolvía antes en el POST; el JSON serializado es idéntico
/// (<c>{"id": ..., "profileId": ...}</c>).
/// </summary>
public record CreateDriverLicenseResponseDto(Guid Id, Guid ProfileId);
