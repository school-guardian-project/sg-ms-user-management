using ms_user_management.Api.Admin.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Admin.Application.Security;

/// <summary>
/// Lectura de un administrador: el propio administrador (pantalla de perfil de
/// la web) o el SuperAdmin (<see cref="PersonAccessGuard"/>).
/// </summary>
public sealed class AccessControlledGetAdminUseCase : IGetPersonUseCase
{
    private readonly IGetPersonUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledGetAdminUseCase(IGetPersonUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task<PersonResponseDto> GetByIdAsync(Guid id)
    {
        await _guard.EnsureCanReadPersonAsync(id, RoleId.Admin);
        return await _inner.GetByIdAsync(id);
    }
}
