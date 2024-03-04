namespace Core.EventModels;

public class TaskCreatedEventModel
{
    public string PublicId { get; set; }
    public string Description { get; set; }
    public string PoPugId { get; set; }
}