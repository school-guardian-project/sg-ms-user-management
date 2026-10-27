using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Student.Domain.Ports.In;

namespace ms_user_management.Api.Student.Application.Security;

/// <summary>Lectura de un estudiante acotada por <see cref="PersonAccessGuard"/>.</summary>
public sealed class AccessControlledGetStudentUseCase : IGetPersonUseCase
{
    private readonly IGetPersonUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledGetStudentUseCase(IGetPersonUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task<PersonResponseDto> GetByIdAsync(Guid id)
    {
        await _guard.EnsureCanReadPersonAsync(id, RoleId.Student);
        return await _inner.GetByIdAsync(id);
    }
}

/// <summary>Listado de estudiantes acotado por <see cref="PersonAccessGuard"/>.</summary>
public sealed class AccessControlledListStudentUseCase : IListStudentUseCase
{
    private readonly IListStudentUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledListStudentUseCase(IListStudentUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task<IEnumerable<PersonListDto>> ExecuteAsync()
    {
        var scope = await _guard.GetPersonListScopeAsync(RoleId.Student);
        return PersonListScope.Apply(await _inner.ExecuteAsync(), scope);
    }
}
