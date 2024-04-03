using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Analytics.Models;

[Table("Tasks", Schema = "Analytics")]
public record TaskDto
{
    [Column(nameof(Id)), Key]
    public long Id { get; set; }
    [Column(nameof(Ulid))]
    public string Ulid { get; set; }
    [Column(nameof(CreatedAt))]
    public DateTime CreatedAt { get; set; }
    [Column(nameof(EditedAt))]
    public DateTime EditedAt { get; set; }
    [Column(nameof(Title))]
    public string Title { get; set; } = string.Empty;
    [Column(nameof(Status))]
    public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Open;
    [Column(nameof(PoPugId))]
    public string PoPugId { get; set; } = string.Empty;
    [Column(nameof(Lose))]
    public decimal? Lose { get; set; }
    [Column(nameof(Reward))]
    public decimal? Reward { get; set; }
    [Column(nameof(JiraId))]
    public string JiraId { get; set; }
}