namespace ms_user_management.Api.Driver.Domain.Event;

public class DriverCreatedEvent
{
    public Guid EventId { get; set; }
    public Guid PersonId { get; set; }
    public string Email { get; set; }
    public string IdentificationNumber { get; set; }
}