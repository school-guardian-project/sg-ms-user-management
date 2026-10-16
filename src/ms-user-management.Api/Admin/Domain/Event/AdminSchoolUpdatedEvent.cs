namespace ms_user_management.Api.Admin.Domain.Event;

public class AdminSchoolUpdatedEvent
{
    public Guid EventId { get; set; }
    public Guid PersonId { get; set; }
    public Guid? SchoolId { get; set; }
}
