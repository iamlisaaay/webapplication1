using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Concert.Models;

public enum TicketStatus
{
    [Display(Name = "Не куплено")]
    NotPurchased,

    [Display(Name = "Куплено")]
    Purchased
}
public partial class Ticket
{
    public int TicketId { get; set; }
    [Display(Name = "Концерт")]
    public int? ConcertId { get; set; }
    [Display(Name = "Покупець")]
    public int? CustomerId { get; set; }
    [Display(Name = "Місце")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public int? SeatNumber { get; set; }
    [Display(Name = "Ряд")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public int? RNumber { get; set; }
    [Display(Name = "Ціна")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public decimal Price { get; set; }
    [Display(Name = "Статус")]
    [Required(ErrorMessage = "Оберіть статус")]
    public TicketStatus Status { get; set; }
    [Display(Name = "Концерт")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public virtual Concert? Concert { get; set; }
    [Display(Name = "Покупець")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public virtual Customer? Customer { get; set; }
}
