namespace ms_user_management.Api.Admin.Domain.Event;

public class AdminCreatedEvent
{
    public Guid EventId { get; set; }
    public Guid PersonId { get; set; }
    public string Email { get; set; }
    public string IdentificationNumber { get; set; }
}