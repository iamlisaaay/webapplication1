using System.ComponentModel.DataAnnotations;

namespace Concert.Models;

public class Comment
{
    public int CommentId { get; set; }

    [Required(ErrorMessage = "Коментар не може бути порожнім")]
    [Display(Name = "Текст коментаря")]
    public string Text { get; set; } = null!;

    [Display(Name = "Дата додавання")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Зв'язок з користувачем
    public int CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    // Зв'язок з концертом
    public int ConcertId { get; set; }
    public virtual Concert? Concert { get; set; }
}