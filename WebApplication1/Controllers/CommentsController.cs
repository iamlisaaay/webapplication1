using Microsoft.AspNetCore.Mvc;
using Concert.Models;
using System;
using System.Threading.Tasks;

namespace Concert.Controllers
{
    // Клас обов'язково має успадковувати Controller
    public class CommentsController : Controller
    {
        private readonly ConcertContext _context;

        // Конструктор: тут ми отримуємо доступ до нашої бази даних (_context)
        public CommentsController(ConcertContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int concertId, string text)
        {
            // Шукаємо ID саме за тим ключем, який ти задала в AccountController
            var userIdClaim = User.FindFirst("UserId");

            // Якщо користувач не залогінений або коментар порожній
            if (userIdClaim == null || string.IsNullOrWhiteSpace(text))
            {
                return RedirectToAction("Details", "Concerts", new { id = concertId });
            }

            // Створюємо коментар
            Concert.Models.Comment newComment = new Concert.Models.Comment
            {
                ConcertId = concertId,
                CustomerId = int.Parse(userIdClaim.Value),
                Text = text,
                CreatedAt = DateTime.UtcNow
            };

            // Зберігаємо
            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            // Повертаємося
            return RedirectToAction("Details", "Concerts", new { id = concertId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int commentId, int concertId, string newText)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || string.IsNullOrWhiteSpace(newText))
                return RedirectToAction("Details", "Concerts", new { id = concertId });

            var userId = User.FindFirst("UserId")?.Value;

            // ТІЛЬКИ автор може редагувати свій коментар
            if (userId != null && comment.CustomerId.ToString() == userId)
            {
                comment.Text = newText;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Concerts", new { id = concertId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int commentId, int concertId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return RedirectToAction("Details", "Concerts", new { id = concertId });

            var userId = User.FindFirst("UserId")?.Value;
            bool isAdmin = User.IsInRole("Admin");

            // Видалити може АБО автор, АБО адмін
            if (isAdmin || (userId != null && comment.CustomerId.ToString() == userId))
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Concerts", new { id = concertId });
        }
    }
}
