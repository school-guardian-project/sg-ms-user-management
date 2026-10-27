namespace ms_user_management.Api.Shared.Domain.Port.In;

/// <summary>Activa o desactiva la cuenta de una persona de cualquier rol.</summary>
public interface IChangePersonStatusUseCase
{
    Task ChangeStatusAsync(Guid personId, string status);
}
