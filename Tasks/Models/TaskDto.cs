using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Tasks.Models;

[Table("Tasks", Schema = "Tasks")]
public record TaskDto
{
    [Column(nameof(Id)), Key]
    public long Id { get; set; }
    [Column(nameof(Ulid))]
    public string Ulid { get; set; } = new Ulid().ToString();
    [Column(nameof(CreatedAt))]
    public DateTime CreatedAt { get; set; }
    [Column(nameof(EditedAt))]
    public DateTime EditedAt { get; set; }
    [Column(nameof(Title))]
    public string Title { get; set; } = string.Empty;
    [Column(nameof(Description))]
    public string? Description { get; set; }
    [Column(nameof(Status))]
    public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Open;
    [Column(nameof(PoPugId))]
    public string PoPugId { get; set; } = string.Empty;
}