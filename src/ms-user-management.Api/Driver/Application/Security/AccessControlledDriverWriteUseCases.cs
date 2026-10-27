using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Driver.Application.Security;

/// <summary>Edicion de un conductor solo dentro del colegio del staff.</summary>
public sealed class AccessControlledUpdateDriverUseCase : IUpdateDriverUseCase
{
    private readonly IUpdateDriverUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledUpdateDriverUseCase(IUpdateDriverUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task UpdateAsync(Guid id, PersonRequestDto dto)
    {
        await _guard.EnsureCanWritePersonAsync(id, RoleId.Driver);
        await _inner.UpdateAsync(id, dto);
    }
}

/// <summary>Baja de un conductor solo dentro del colegio del staff.</summary>
public sealed class AccessControlledDeleteDriverUseCase : IDeleteDriverUseCase
{
    private readonly IDeleteDriverUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledDeleteDriverUseCase(IDeleteDriverUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _guard.EnsureCanWritePersonAsync(id, RoleId.Driver);
        await _inner.DeleteAsync(id);
    }
}
