namespace ms_user_management.Api.Driver.Domain.Ports.In;

public interface IDeleteDriverUseCase
{
    Task DeleteAsync(Guid id);
}
