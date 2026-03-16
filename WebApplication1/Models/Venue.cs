using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Concert.Models;

public partial class Venue
{
    public int VenueId { get; set; }
    [Display(Name = "Назва")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string Name { get; set; } = null!;
    [Display(Name = "Адреса")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string? Address { get; set; }
    [Display(Name = "Кількість стояих місць")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public int? Capacity { get; set; }
    [Display(Name = "Кількість місць")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public int? TotalSeats { get; set; }
    [Display(Name = "Кількість рядів")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public int? TotalRows { get; set; }
    [Display(Name = "Фото локації (посилання)")]
    public string? ImageUrl { get; set; }

    public virtual ICollection<Concert> Concerts { get; set; } = new List<Concert>();
}
