using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Concert.Models;

public partial class Concert
{
    private System.DateTime? _dateTime;
    [Display(Name = "Дата")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public System.DateTime? DateTime
    {
        get => _dateTime;
        set => _dateTime = value.HasValue
            ? System.DateTime.SpecifyKind(value.Value, System.DateTimeKind.Utc)
            : null;
    }

    public int ConcertId { get; set; }
    [Display(Name = "Назва")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string Title { get; set; } = null!;


    [Display(Name = "Веню")]
    public int? VenueId { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    [Display(Name = "Веню")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public virtual Venue? Venue { get; set; }
    [Display(Name = "Посилання на афішу")]
   
    public string? ImageUrl { get; set; }
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}
