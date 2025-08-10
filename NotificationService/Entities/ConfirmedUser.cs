using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationService.Entities;

[Table("ConfirmedUsers")]
[PrimaryKey(nameof(UserName))]
public class ConfirmedUser
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
}
