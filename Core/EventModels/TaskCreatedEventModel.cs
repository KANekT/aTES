namespace Core.EventModels;

public class TaskCreatedEventModel
{
    public string PublicId { get; set; }
    public string Description { get; set; }
    public int Lose { get; set; }
    public int Reward { get; set; }
}