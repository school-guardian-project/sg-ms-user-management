namespace ms_user_management.Api.Family.Application.Dto;

/// <summary>
/// Perfil de un acudiente asociado a un estudiante. El JSON serializado es
/// idéntico al del objeto anónimo anterior: <c>{"profileId": ...}</c>.
/// </summary>
public record FamilyMemberProfileDto(Guid ProfileId);
