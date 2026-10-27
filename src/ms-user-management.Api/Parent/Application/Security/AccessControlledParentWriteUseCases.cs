using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Parent.Application.Security;

/// <summary>Edicion de un acudiente solo dentro del colegio del staff.</summary>
public sealed class AccessControlledUpdateParentUseCase : IUpdateParentUseCase
{
    private readonly IUpdateParentUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledUpdateParentUseCase(IUpdateParentUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task UpdateAsync(Guid id, PersonRequestDto dto)
    {
        await _guard.EnsureCanWritePersonAsync(id, RoleId.Parent);
        await _inner.UpdateAsync(id, dto);
    }
}

/// <summary>Baja de un acudiente solo dentro del colegio del staff.</summary>
public sealed class AccessControlledDeleteParentUseCase : IDeleteParentUseCase
{
    private readonly IDeleteParentUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledDeleteParentUseCase(IDeleteParentUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _guard.EnsureCanWritePersonAsync(id, RoleId.Parent);
        await _inner.DeleteAsync(id);
    }
}
