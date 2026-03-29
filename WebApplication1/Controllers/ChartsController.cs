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

        // Дані для сторінки концертів 
        [HttpGet("revenueByConcert")]
        public async Task<IActionResult> GetRevenueByConcertAsync(CancellationToken cancellationToken)
        {
            var revenueData = await _context.Tickets
                .Where(t => t.Status == TicketStatus.Purchased)
                .GroupBy(t => t.Concert.Title)
                .Select(g => new {
                    concertTitle = g.Key,
                    totalRevenue = g.Sum(t => t.Price)
                })
                .ToListAsync(cancellationToken);

            return Json(revenueData);
        }

        // Дані для сторінки гуртів
        [HttpGet("groupActivity")]
        public async Task<IActionResult> GetGroupActivityAsync(CancellationToken cancellationToken)
        {
            var groupData = await _context.Groups
                .Select(g => new {
                    groupName = g.Name,
                    concertCount = g.Concerts.Count
                })
                .Where(g => g.concertCount > 0)
                .ToListAsync(cancellationToken);

            return Json(groupData);
        }
    }
}