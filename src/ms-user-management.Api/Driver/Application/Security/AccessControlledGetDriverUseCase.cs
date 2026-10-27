using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Driver.Application.Security;

/// <summary>
/// Lectura de un conductor: el propio conductor o el staff con drivers.read de
/// su colegio (<see cref="PersonAccessGuard"/>).
/// </summary>
public sealed class AccessControlledGetDriverUseCase : IGetPersonUseCase
{
    private readonly IGetPersonUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledGetDriverUseCase(IGetPersonUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task<PersonResponseDto> GetByIdAsync(Guid id)
    {
        await _guard.EnsureCanReadPersonAsync(id, RoleId.Driver);
        return await _inner.GetByIdAsync(id);
    }
}
