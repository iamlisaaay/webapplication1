using System.ComponentModel.DataAnnotations;

namespace Concert.Models;

public class EditProfileViewModel
{
    [Display(Name = "ПІБ")]
    [Required(ErrorMessage = "Введіть ваше ім'я")]
    public string FullName { get; set; } = null!;

    [Display(Name = "Електронна пошта")]
    [Required(ErrorMessage = "Пошта обов'язкова")]
    [EmailAddress(ErrorMessage = "Некоректний формат пошти")]
    public string Email { get; set; } = null!;

    [Display(Name = "Дата народження")]
    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; }
}