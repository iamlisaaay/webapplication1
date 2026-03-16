using Concert.Models; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Models;

namespace Concert.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConcertContext _context; 

   
        public HomeController(ILogger<HomeController> logger, ConcertContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
           
            var upcomingConcerts = await _context.Concerts
                .Include(c => c.Venue)
                .Where(c => c.DateTime >= DateTime.UtcNow)
                .OrderBy(c => c.DateTime) 
                .Take(6) 
                .ToListAsync();

            return View(upcomingConcerts); 
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}