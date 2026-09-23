using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

public class PersonEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public String Name { get; set; }
    public String LastName { get; set; }
    public IdentificationType IdentificationType { get; set; }
    public String IdentificationNumber { get; set; }
    public String Email { get; set; }
    public int Phone { get; set; }
    public String ResidenceAddress { get; set; }
    public DateOnly DateBirth { get; set; }
    public Status Status { get; set; }
}