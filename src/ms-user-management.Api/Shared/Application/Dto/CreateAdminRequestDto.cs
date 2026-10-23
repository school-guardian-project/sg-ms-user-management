namespace ms_user_management.Api.Shared.Application.Dto;

/// <summary>
/// Alta de admin.
///
/// <para>
/// Un admin pertenece a un <em>colegio</em>, no a una sede: por eso esta clase
/// lleva <see cref="SchoolId"/> y no <c>CampusId</c>. La relacion se materializa
/// en <c>School.SchoolAdmin</c>, que ms-iam escribe por gRPC despues de crear el
/// perfil.
/// </para>
/// </summary>
public class CreateAdminRequestDto : PersonRequestDto
{
}
