namespace Gateway.DTOs.Admin.Subject;

public class UpdateSubjectRequest
{
    public string Id { get; init; }
    
    public string Owner { get; init; }
    
    public string Name { get; init; }
    
    public List<string> Prerequisites  { get; init; }
    
    public List<string> Courses { get; init; }
}