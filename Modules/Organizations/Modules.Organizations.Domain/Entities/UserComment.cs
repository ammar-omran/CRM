namespace Modules.Organizations.Domain.Entities;

public class UserComment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public bool ValidateUserComment()
    {
        if (string.IsNullOrEmpty(CreatedByName) || CreatedByName.Length > 150)
            return false;
        if (string.IsNullOrEmpty(Content) || Content.Length > 1000)
            return false;
        if (TicketId <= 0)
            return false;
        if (CreatedBy <= 0)
            return false;
        return true;
    }
}
