namespace ms_user_management.Api.Student.Domain.Event;

public class StudentCreatedEvent
{
    public Guid EventId { get; set; }
    public Guid PersonId { get; set; }
    public string Email { get; set; }
    public string IdentificationNumber { get; set; }
}