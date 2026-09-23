namespace ms_user_management.Api.Student.Domain.Ports.In;

public interface IDeleteStudentUseCase
{
    Task DeleteAsync(Guid id);
}