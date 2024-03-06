using Core.Enums;

namespace Core.EventModels;

public class TaskMutationEventModel
{
    public string PublicTaskId { get; set; }
    public string PublicPoPugId { get; set; }
    public TaskMutationEnum Status { get; set; }
}