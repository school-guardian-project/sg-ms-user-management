using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Family.Domain.Model;

public class Family
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public String Name { get; set; }
    public String Observations { get; set; }
    public Status Status { get; set; }
}