using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Student.Domain.Ports.In;

namespace ms_user_management.Api.Student.Application.Security;

/// <summary>Edicion de un estudiante solo dentro del colegio del staff.</summary>
public sealed class AccessControlledUpdateStudentUseCase : IUpdateStudentUseCase
{
    private readonly IUpdateStudentUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledUpdateStudentUseCase(IUpdateStudentUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task UpdateAsync(Guid id, PersonRequestDto dto)
    {
        await _guard.EnsureCanWritePersonAsync(id, RoleId.Student);
        await _inner.UpdateAsync(id, dto);
    }
}

/// <summary>Baja de un estudiante solo dentro del colegio del staff.</summary>
public sealed class AccessControlledDeleteStudentUseCase : IDeleteStudentUseCase
{
    private readonly IDeleteStudentUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledDeleteStudentUseCase(IDeleteStudentUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _guard.EnsureCanWritePersonAsync(id, RoleId.Student);
        await _inner.DeleteAsync(id);
    }
}
