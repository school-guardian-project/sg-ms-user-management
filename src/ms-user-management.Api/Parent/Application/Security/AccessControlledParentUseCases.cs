using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Parent.Application.Security;

/// <summary>Lectura de un acudiente acotada por <see cref="PersonAccessGuard"/>.</summary>
public sealed class AccessControlledGetParentUseCase : IGetPersonUseCase
{
    private readonly IGetPersonUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledGetParentUseCase(IGetPersonUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task<PersonResponseDto> GetByIdAsync(Guid id)
    {
        await _guard.EnsureCanReadPersonAsync(id, RoleId.Parent);
        return await _inner.GetByIdAsync(id);
    }
}

/// <summary>Listado de acudientes acotado por <see cref="PersonAccessGuard"/>.</summary>
public sealed class AccessControlledListParentUseCase : IListParentUseCase
{
    private readonly IListParentUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledListParentUseCase(IListParentUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task<IEnumerable<PersonListDto>> ExecuteAsync()
    {
        var scope = await _guard.GetPersonListScopeAsync(RoleId.Parent);
        return PersonListScope.Apply(await _inner.ExecuteAsync(), scope);
    }
}
