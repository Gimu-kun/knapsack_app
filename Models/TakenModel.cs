using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knapsack_app.Models;

[Table("taken")]
public class TakenModel
{
    [Key]
    [Column("id", TypeName = "varchar(50)")]
    public string Id { get; set; } = "TK-" + Guid.NewGuid().ToString().Replace("-", "")[..8].ToUpper();

    [Required]
    [Column("user_id", TypeName = "varchar(50)")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column("challenge_id", TypeName = "varchar(50)")]
    public string ChallengeId { get; set; } = string.Empty;

    [Column("is_multiplay", TypeName = "tinyint(1)")]
    public bool IsMultiplay { get; set; }

    [Column("player_count")]
    public int PlayerCount { get; set; }

    [Column("taken_score")]
    public int TakenScore { get; set; }

    [Column("taken_duration")]
    public int TakenDuration { get; set; }

    [Column("taken_at")]
    public DateTime TakenAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual UserModel User { get; set; } = null!;

    [ForeignKey(nameof(ChallengeId))]
    public virtual KsChallengeModel Challenge { get; set; } = null!; // Giả định tên model là KsChallengeModel


    public TakenModel() 
    {
        TakenAt = DateTime.UtcNow;
    }
    
    public TakenModel(string userId, string challengeId, bool isMultiplay, int playerCount, int takenScore, int takenDuration)
    {
        UserId = userId;
        ChallengeId = challengeId;
        IsMultiplay = isMultiplay;
        PlayerCount = playerCount;
        TakenScore = takenScore;
        TakenDuration = takenDuration;
        TakenAt = DateTime.UtcNow;
    }
}