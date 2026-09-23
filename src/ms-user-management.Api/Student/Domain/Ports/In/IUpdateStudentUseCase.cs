using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Student.Domain.Ports.In;

public interface IUpdateStudentUseCase
{
    Task UpdateAsync(Guid id, PersonRequestDto dto);
}