using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Concert.Models;

public partial class Role
{
    public int RoleId { get; set; }

    [Display(Name = "Назва ролі")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string RoleName { get; set; } = null!;

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}