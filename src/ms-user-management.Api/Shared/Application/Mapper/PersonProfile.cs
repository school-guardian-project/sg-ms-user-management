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
            .ForMember(dest => dest.CityId, opt => opt.Condition(src => src.CityId.HasValue))
            .ForMember(dest => dest.SchoolId, opt => opt.Condition(src => src.SchoolId.HasValue))
            .ForMember(
                dest => dest.IdentificationType,
                opt => opt.MapFrom(src =>
                    Enum.Parse<IdentificationType>(
                        src.IdentificationType,
                        true
                    )
                )
            )
            // Los DTO de alta heredan de PersonRequestDto y agregan campusId o
            // schoolId, que no son columnas de Person. Include deja que ambos
            // reutilicen la conversion de IdentificationType sin duplicarla.
            .Include<CreatePersonRequestDto, Person>()
            .Include<CreateAdminRequestDto, Person>();

        CreateMap<CreatePersonRequestDto, Person>()
            .IncludeBase<PersonRequestDto, Person>();

        CreateMap<CreateAdminRequestDto, Person>()
            .IncludeBase<PersonRequestDto, Person>();
    }
}