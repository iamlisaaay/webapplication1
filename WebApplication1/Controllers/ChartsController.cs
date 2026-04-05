using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Concert.Models;

namespace Concert.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : Controller
    {
        private readonly ConcertContext _context;

        public ChartsController(ConcertContext context)
        {
            _context = context;
        }

        // 1. Отримання списку гуртів для випадаючого списку
        [HttpGet("groups")]
        public async Task<IActionResult> GetGroupsAsync(CancellationToken cancellationToken)
        {
            var groups = await _context.Groups
                .Select(g => new {
                    id = g.GroupId, // Переконайся, що первинний ключ називається GroupId (або зміни на Id, якщо він так називається)
                    name = g.Name
                })
                .ToListAsync(cancellationToken);

            return Json(groups);
        }

        // 2. Графік №1: Продані квитки та зароблені гроші по місяцях (з фільтром)
        [HttpGet("monthlySales")]
        public async Task<IActionResult> GetMonthlySalesAsync([FromQuery] int? groupId, CancellationToken cancellationToken)
        {
            var query = _context.Tickets
                .Include(t => t.Concert)
                .Where(t => t.Status == TicketStatus.Purchased && t.Concert.DateTime.HasValue);

            // Фільтр по гурту
            if (groupId.HasValue && groupId.Value > 0)
            {
                query = query.Where(t => t.Concert.Groups.Any(g => g.GroupId == groupId.Value));
            }

            var data = await query
                .GroupBy(t => new {
                    Year = t.Concert.DateTime.Value.Year,
                    Month = t.Concert.DateTime.Value.Month
                })
                .Select(g => new {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    ticketsSold = g.Count(),
                    revenue = g.Sum(t => t.Price)
                })
                .OrderBy(x => x.year).ThenBy(x => x.month)
                .ToListAsync(cancellationToken);

            // Форматування для JS
            var result = data.Select(d => new {
                monthLabel = $"{d.year}-{d.month:D2}",
                ticketsSold = d.ticketsSold,
                revenue = d.revenue
            });

            return Json(result);
        }

        // 3. Графік №2: Втрачені гроші (непродані квитки)
        [HttpGet("lostRevenue")]
        public async Task<IActionResult> GetLostRevenueAsync(CancellationToken cancellationToken)
        {
            var lostData = await _context.Tickets
                .Include(t => t.Concert)
                .Where(t => t.Status == TicketStatus.NotPurchased) // Рахуємо тільки некуплені квитки
                .GroupBy(t => t.Concert.Title)
                .Select(g => new {
                    concertTitle = g.Key,
                    lostRevenue = g.Sum(t => t.Price)
                })
                .OrderByDescending(x => x.lostRevenue)
                .ToListAsync(cancellationToken);

            return Json(lostData);
        }
    }
}