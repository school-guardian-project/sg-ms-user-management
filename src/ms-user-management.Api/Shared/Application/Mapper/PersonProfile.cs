using AutoMapper;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Application.Mapper;

public class PersonProfile : Profile
{
    public PersonProfile()
    {
        CreateMap<Person, PersonListDto>();
        CreateMap<Person, PersonResponseDto>();
        CreateMap<PersonRequestDto, Person>()
            .ForMember(
                dest => dest.IdentificationType,
                opt => opt.MapFrom(src =>
                    Enum.Parse<IdentificationType>(
                        src.IdentificationType,
                        true
                    )
                )
            );
    }
}