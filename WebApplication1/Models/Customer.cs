using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Concert.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    [Display(Name = "Ім'я")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    public string FullName { get; set; } = null!;
    [Display(Name = "Пошта")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]
    [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Некоректний формат пошти (приклад: name@email.com)")]
    public string? Email { get; set; }

    [Display(Name = "Дата народження")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]

    [DataType(DataType.Date)]

    public DateOnly? BirthDate { get; set; }

    [Display(Name = "Знижка")]
    [Required(ErrorMessage = "Поле не повинне бути порожнім")]

    [Range(0, 50, ErrorMessage = "Знижка має бути від 0 до 50 відсотків")]
    public decimal? LoyaltyDiscount { get; set; }


    [Display(Name = "Пароль")]
    [Required(ErrorMessage = "Пароль обов'язковий")]
    public string Password { get; set; } = null!;

    [Display(Name = "Чи є Адміном?")]
    public bool IsAdmin { get; set; } = false;
    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiry { get; set; }
    [Display(Name = "Аватар")]
    public string? AvatarUrl { get; set; }
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}