using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Concert.Models;
using Microsoft.AspNetCore.Authorization;

namespace Concert.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ChartsController : Controller
    {
        private readonly ConcertContext _context;

        public ChartsController(ConcertContext context)
        {
            _context = context;
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetGroupsAsync(CancellationToken cancellationToken)
        {
            var groups = await _context.Groups
                .Select(g => new { id = g.GroupId, name = g.Name })
                .ToListAsync(cancellationToken);
            return Json(groups);
        }

        [HttpGet("monthlySales")]
        public async Task<IActionResult> GetMonthlySalesAsync(
            [FromQuery] int? groupId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken)
        {
            if (startDate.HasValue) startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            if (endDate.HasValue) endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
     
            var query = _context.Tickets
                .Include(t => t.Concert)
                .Where(t => t.Status == TicketStatus.Purchased && t.Concert.DateTime.HasValue);

            if (groupId.HasValue && groupId.Value > 0)
                query = query.Where(t => t.Concert.Groups.Any(g => g.GroupId == groupId.Value));

            if (startDate.HasValue) query = query.Where(t => t.Concert.DateTime >= startDate.Value);
            if (endDate.HasValue) query = query.Where(t => t.Concert.DateTime <= endDate.Value);

            var data = await query
                .GroupBy(t => new { Year = t.Concert.DateTime.Value.Year, Month = t.Concert.DateTime.Value.Month })
                .Select(g => new {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    ticketsSold = g.Count(),
                    revenue = g.Sum(t => t.Price)
                })
                .OrderBy(x => x.year).ThenBy(x => x.month)
                .ToListAsync(cancellationToken);

            return Json(data.Select(d => new { monthLabel = $"{d.year}-{d.month:D2}", d.ticketsSold, d.revenue }));
        }

        [HttpGet("lostRevenue")]
        public async Task<IActionResult> GetLostRevenueAsync(
            [FromQuery] int? groupId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken)
        {
            if (startDate.HasValue) startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            if (endDate.HasValue) endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
            // ======================
            var query = _context.Tickets.Include(t => t.Concert).Where(t => t.Concert.DateTime.HasValue);

            if (groupId.HasValue && groupId.Value > 0)
                query = query.Where(t => t.Concert.Groups.Any(g => g.GroupId == groupId.Value));

            if (startDate.HasValue) query = query.Where(t => t.Concert.DateTime >= startDate.Value);
            if (endDate.HasValue) query = query.Where(t => t.Concert.DateTime <= endDate.Value);

            var data = await query
                .GroupBy(t => new { Year = t.Concert.DateTime.Value.Year, Month = t.Concert.DateTime.Value.Month })
                .Select(g => new {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    lostRevenue = g.Where(t => t.Status == TicketStatus.NotPurchased).Sum(t => t.Price),
                    earnedRevenue = g.Where(t => t.Status == TicketStatus.Purchased).Sum(t => t.Price)
                })
                .OrderBy(x => x.year).ThenBy(x => x.month)
                .ToListAsync(cancellationToken);

            return Json(data.Select(d => new { monthLabel = $"{d.year}-{d.month:D2}", d.lostRevenue, d.earnedRevenue }));
        }
    }
}