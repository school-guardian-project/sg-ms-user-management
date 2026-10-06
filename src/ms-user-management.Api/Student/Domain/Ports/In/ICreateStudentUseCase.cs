using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Student.Application;

namespace ms_user_management.Api.Student.Domain.Ports.In;

public interface ICreateStudentUseCase
{
    Task CreateAsync(PersonRequestDto dto);
}