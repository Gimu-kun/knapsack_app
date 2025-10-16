using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knapsack_app.Models
{
    [Table("user_session_log")]
    public class UserSessionLogModel
    {
        [Key]
        [Column("id", TypeName = "varchar(50)")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("user_id", TypeName = "varchar(50)")]
        public string UserId { get; set; } = string.Empty;

        [Column("login_time")]
        public DateTime LoginTime { get; set; } = DateTime.UtcNow; 

        [Column("logout_time")]
        public DateTime? LogoutTime { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserModel User { get; set; } = null!;
    }
}