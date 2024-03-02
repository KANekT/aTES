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
    public string Ulid { get; set; }
    [Column(nameof(CreatedAt))]
    public DateTime CreatedAt { get; set; }
    [Column(nameof(EditedAt))]
    public DateTime EditedAt { get; set; }
    [Column(nameof(Description))]
    public string Description { get; set; } = string.Empty;
    [Column(nameof(Status))]
    public StatusEnum Status { get; set; } = StatusEnum.Open;
    [Column(nameof(PoPugId))]
    public string PoPugId { get; set; } = string.Empty;
}

public record TaskPrice
{
    [Column(nameof(Lose))]
    public int Lose { get; private set; } = new Random().Next(10, 20);
    [Column(nameof(Reward))]
    public int Reward { get; private set; } = new Random().Next(20, 40);
}