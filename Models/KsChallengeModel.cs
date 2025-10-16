using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace knapsack_app.Models;

[Table("ks_challenge")]
public class KsChallengeModel
{
    [Key]
    [Column("id", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string Id { get; set; } = "KSC-" + Guid.NewGuid().ToString("N")[..6].ToUpper();

    [Column("difficulty", TypeName = "varchar(20)")]
    [Required]
    public DifficultyEnum Difficulty { get; set; }

    [Column("ques_data", TypeName = "json")]
    [Required]
    public string QuesData { get; set; } = string.Empty;

    [Column("dp_board", TypeName = "json")]
    [Required]
    public string DpBoard { get; set; } = string.Empty;

    [Column("result_items", TypeName = "json")]
    public string? ResultItems { get; set; }

    [Column("max_capacity")]
    public int? MaxCapacity { get; set; }

    [Column("max_duration")]
    public int? MaxDuration { get; set; } = 3000;

    [Column("miss_count")]
    public int? MissCount { get; set; }

    [Column("miss_array", TypeName = "json")]
    public string? MissArray { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    [Column("updated_by", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? UpdatedBy { get; set; }

    public virtual ICollection<TakenModel> TakenRecords { get; set; } = new List<TakenModel>();

    [ForeignKey(nameof(CreatedBy))]
    public virtual UserModel? Creator { get; set; }

    [ForeignKey(nameof(UpdatedBy))]
    public virtual UserModel? Updater { get; set; }

    public KsChallengeModel() { }
}