using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Concert.Models;

public partial class Member
{
    public int MemberId { get; set; }
    [Display(Name = "Ім'я")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string FullName { get; set; } = null!;
    [Display(Name = "Фото (посилання)")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Instagram (посилання)")]
    public string? InstagramUrl { get; set; }
    public int? Role { get; set; }
    [Display(Name = "Роль")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public virtual Role? RoleNavigation { get; set; }

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}
