using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace knapsack_app.Models;

[Table("admin")] // Giả định tên bảng trong CSDL là 'admin'
public class AdminModel
{
    [Key]
    [Column("id", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string Id { get; set; } = "AD-" + Guid.NewGuid().ToString().Replace("-", "")[..8].ToUpper();

    [Required]
    [Column("account", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string Account { get; set; } = string.Empty;

    [Required]
    [Column("passwords", TypeName = "varchar(100)")]
    [MaxLength(100)]
    public string Passwords { get; set; } = string.Empty;

    [Column("avatar", TypeName = "longtext")]
    public string? Avatar { get; set; } = "https://i2.wp.com/vdostavka.ru/wp-content/uploads/2019/05/no-avatar.png?fit=512%2C512&ssl=1";

    [Column("role", TypeName = "tinyint(1)")]
    public bool Role { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by", TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public virtual AdminModel? Creator { get; set; }

    public AdminModel() { }

    public AdminModel(string account, string passwords, string createdBy, string? avatar = null, bool role = false)
    {
        Account = account;
        Passwords = passwords;
        CreatedBy = createdBy;
        
        if (!string.IsNullOrEmpty(avatar))
        {
            Avatar = avatar;
        }
        Role = role;
    }
}