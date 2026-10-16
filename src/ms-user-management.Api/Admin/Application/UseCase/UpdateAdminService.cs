using AutoMapper;
using ms_user_management.Api.Admin.Domain.Ports.In;
using ms_user_management.Api.Admin.Domain.Event;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Admin.Application.UseCase;

public class UpdateAdminService : IUpdateAdminUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public UpdateAdminService(IPersonRepository personRepository, IMapper mapper, IEventPublisher eventPublisher)
    {
        _personRepository = personRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task UpdateAsync(Guid id, PersonRequestDto dto)
    {
        var person = await _personRepository.GetByIdAsync(id);

        if (person is null) throw new Exception("Not Found");

        _mapper.Map(dto, person);

        await _personRepository.UpdateAsync(person);
        await _eventPublisher.PublishAsync("admin.school.updated", new AdminSchoolUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.Id,
            SchoolId = person.SchoolId
        });
    }
}
