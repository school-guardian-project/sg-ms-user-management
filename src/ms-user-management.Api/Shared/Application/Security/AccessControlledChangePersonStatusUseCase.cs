using ms_user_management.Api.Shared.Domain.Port.In;

namespace ms_user_management.Api.Shared.Application.Security;

/// <summary>
/// Cambio de estado solo de personas del colegio del staff; las cuentas de
/// administrador solo las gestiona el SuperAdmin.
/// </summary>
public sealed class AccessControlledChangePersonStatusUseCase : IChangePersonStatusUseCase
{
    private readonly IChangePersonStatusUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledChangePersonStatusUseCase(IChangePersonStatusUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task ChangeStatusAsync(Guid personId, string status)
    {
        await _guard.EnsureCanWriteAnyPersonAsync(personId);
        await _inner.ChangeStatusAsync(personId, status);
    }
}
