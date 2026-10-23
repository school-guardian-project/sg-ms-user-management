namespace ms_user_management.Api.Shared.Application.Dto;

/// <summary>
/// Datos comunes de una persona. Se mantiene aparte de los DTOs de alta
/// porque <c>PUT</c> no lleva <c>campusId</c>: la sede se asigna al dar de alta
/// y cambiarla es un caso distinto (un traslado de sede), no un Simple update.
/// </summary>
public class PersonRequestDto
{
    public string Name { get; set; }
    public string LastName { get; set; }
    public string IdentificationType { get; set; }
    public string IdentificationNumber { get; set; }
    public string Email { get; set; }
    public long Phone { get; set; }
    public string ResidenceAddress { get; set; }
    public DateOnly DateBirth { get; set; }
    public Guid? CityId { get; set; }
    public Guid? SchoolId { get; set; }
}
