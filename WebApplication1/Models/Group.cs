using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Concert.Models;

public partial class Group
{
    public int GroupId { get; set; }
    [Display(Name = "Назва")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string Name { get; set; } = null!;
    [Display(Name = "Опис")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string? Description { get; set; }
    [Display(Name = "Посилання на лого")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string? LogoUrl { get; set; }
    [Display(Name = "Посилання на фон")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string? BgVideoUrl { get; set; }

    [Display(Name = "Посилання на фото групи")]
    public string? VideoUrl { get; set; }
    [Display(Name = "Посилання на фан клуб")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string? FanClubUrl { get; set; }

    public virtual ICollection<Concert> Concerts { get; set; } = new List<Concert>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
