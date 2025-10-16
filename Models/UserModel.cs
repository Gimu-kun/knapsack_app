using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knapsack_app.Models;

[Table("user")]
public class UserModel
{
    [Key]
    [Column("id", TypeName = "varchar(50)")]
    public string Id { get; set; } = "U-" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper();

    [Required]
    [Column("account", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string Account { get; set; } = string.Empty;

    [Required]
    [Column("passwords", TypeName = "varchar(100)")]
    [MaxLength(100)]
    public string Passwords { get; set; } = string.Empty;
    
    [Column("status", TypeName = "tinyint(1)")]
    public bool Status { get; set; } = true;

    [Column("avatar", TypeName = "longtext")]
    public string? Avatar { get; set; } = "https://i2.wp.com/vdostavka.ru/wp-content/uploads/2019/05/no-avatar.png?fit=512%2C512&ssl=1";
    [Column("last_login")]
    public DateTime? LastLogin { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    
    public virtual ICollection<TakenModel> TakenChallenges { get; set; } = new List<TakenModel>();

    public virtual ICollection<UserSessionLogModel> SessionLogs { get; set; } = new List<UserSessionLogModel>();

    public UserModel() { }

    public UserModel(string account, string passwords, string? avatar = null)
    {
        Account = account;
        Passwords = passwords;
        
        if (!string.IsNullOrEmpty(avatar))
        {
            Avatar = avatar;
        }
    }
}