using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Student.Domain.Ports.In;

public interface ISearchStudentUseCase
{
    Task<IEnumerable<PersonListDto>> ExecuteAsync(string search);
}