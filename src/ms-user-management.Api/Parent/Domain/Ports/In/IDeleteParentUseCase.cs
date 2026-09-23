namespace ms_user_management.Api.Parent.Domain.Ports.In;

public interface IDeleteParentUseCase
{
    Task DeleteAsync(Guid id);
}
