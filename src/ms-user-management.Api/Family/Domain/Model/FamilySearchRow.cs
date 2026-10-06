namespace ms_user_management.Api.Family.Domain.Model;

public class FamilySearchRow
{
    public Guid FamilyId { get; set; }
    public String FamilyName { get; set; } = string.Empty;
    public String Observations { get; set; } = string.Empty;
    public Guid? ParentProfileId { get; set; }
    public String GuardianName { get; set; } = string.Empty;
    public String GuardianLastName { get; set; } = string.Empty;
    public int GuardianPhone { get; set; }
}
