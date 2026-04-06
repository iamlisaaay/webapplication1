using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Concert.Models;
using System.Text.Json;

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

        // 1. Отримання списку груп
        [HttpGet("groups")]
        public async Task<IActionResult> GetGroupsAsync(CancellationToken cancellationToken)
        {
            var groups = await _context.Groups
                .Select(g => new {
                    id = g.GroupId,
                    name = g.Name
                })
                .ToListAsync(cancellationToken);

            return Json(groups);
        }

        // 2. Дані для графіка продажів (Квитки + Гроші)
        [HttpGet("monthlySales")]
        public async Task<IActionResult> GetMonthlySalesAsync([FromQuery] int? groupId, CancellationToken cancellationToken)
        {
            var query = _context.Tickets
                .Include(t => t.Concert)
                .Where(t => t.Status == TicketStatus.Purchased && t.Concert.DateTime.HasValue);

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

            var result = data.Select(d => new {
                monthLabel = $"{d.year}-{d.month:D2}",
                ticketsSold = d.ticketsSold,
                revenue = d.revenue
            });

            return Json(result);
        }

        // 3. Дані для графіка порівняння (Зароблено vs Втрачено)
        [HttpGet("lostRevenue")]
        public async Task<IActionResult> GetLostRevenueAsync([FromQuery] int? groupId, CancellationToken cancellationToken)
        {
            var query = _context.Tickets
                .Include(t => t.Concert)
                .Where(t => t.Concert.DateTime.HasValue);

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
                    lostRevenue = g.Where(t => t.Status == TicketStatus.NotPurchased).Sum(t => t.Price),
                    earnedRevenue = g.Where(t => t.Status == TicketStatus.Purchased).Sum(t => t.Price)
                })
                .OrderBy(x => x.year).ThenBy(x => x.month)
                .ToListAsync(cancellationToken);

            var result = data.Select(d => new {
                monthLabel = $"{d.year}-{d.month:D2}",
                lostRevenue = d.lostRevenue,
                earnedRevenue = d.earnedRevenue
            });

            return Json(result);
        }
    }
}