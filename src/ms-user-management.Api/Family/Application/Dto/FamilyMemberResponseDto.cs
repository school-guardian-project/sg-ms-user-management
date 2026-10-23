namespace ms_user_management.Api.Family.Application.Dto;

/// <summary>
/// Miembro devuelto tras el registro de una familia. El JSON serializado es
/// idéntico al del objeto anónimo anterior: <c>{"memberId": ..., "profileId": ...}</c>.
/// </summary>
public record FamilyMemberResponseDto(Guid MemberId, Guid ProfileId);
