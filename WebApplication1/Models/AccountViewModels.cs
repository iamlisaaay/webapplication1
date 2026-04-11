using System.ComponentModel.DataAnnotations;

namespace Concert.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введіть пошту")]
    [EmailAddress(ErrorMessage = "Некоректна пошта")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть ПІБ")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Введіть пошту")]
    [EmailAddress(ErrorMessage = "Некоректна пошта")]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "Оберіть дату народження")]
    [DataType(DataType.Date)]
    public DateOnly BirthDate { get; set; }
    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Повторіть пароль")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = null!;
}