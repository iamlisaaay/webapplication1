using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Concert.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Concert.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ConcertContext _context;

        public TicketsController(ConcertContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            IQueryable<Ticket> ticketsQuery = _context.Tickets
                .Include(t => t.Concert)
                .Include(t => t.Customer);

            if (!User.IsInRole("Admin"))
            {
                var userIdStr = User.FindFirstValue("UserId");
                if (int.TryParse(userIdStr, out int userId))
                {
                    ticketsQuery = ticketsQuery.Where(t => t.CustomerId == userId);
                }
                else
                {
                    ticketsQuery = ticketsQuery.Where(t => false);
                }
            }

            var tickets = await ticketsQuery.ToListAsync();

            var ticketStats = await _context.Groups
                .Select(g => new
                {
                    GroupName = g.Name,
                    TicketsSold = g.Concerts.SelectMany(c => c.Tickets).Count()
                })
                .OrderByDescending(g => g.TicketsSold)
                .Take(10)
                .ToListAsync();

            ViewBag.ChartLabels = JsonSerializer.Serialize(ticketStats.Select(s => s.GroupName));
            ViewBag.ChartData = JsonSerializer.Serialize(ticketStats.Select(s => s.TicketsSold));

            return View(tickets);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Concert)
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(m => m.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userIdStr = User.FindFirstValue("UserId");
                if (!int.TryParse(userIdStr, out int userId) || ticket.CustomerId != userId)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }

            return View(ticket);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ConcertId"] = new SelectList(_context.Concerts, "ConcertId", "Title");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("TicketId,ConcertId,CustomerId,SeatNumber,RNumber,Price,Status")] Ticket ticket)
        {
            ModelState.Remove("Concert");
            ModelState.Remove("Customer");
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConcertId"] = new SelectList(_context.Concerts, "ConcertId", "Title", ticket.ConcertId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName", ticket.CustomerId);
            return View(ticket);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["ConcertId"] = new SelectList(_context.Concerts, "ConcertId", "ConcertId", ticket.ConcertId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", ticket.CustomerId);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,ConcertId,CustomerId,SeatNumber,RNumber,Price,Status")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }
            ModelState.Remove("Concert");
            ModelState.Remove("Customer");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConcertId"] = new SelectList(_context.Concerts, "ConcertId", "ConcertId", ticket.ConcertId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", ticket.CustomerId);
            return View(ticket);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Concert)
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ClearAllTickets()
        {
            await _context.Tickets.ExecuteDeleteAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateTestData()
        {
            var concerts = await _context.Concerts.ToListAsync();
            var customers = await _context.Customers.ToListAsync();

            if (!customers.Any())
            {
                var newCustomer = new Customer
                {
                    FullName = "Тестовий Покупець",
                    Email = "test@example.com",
                    BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                    LoyaltyDiscount = 0
                };
                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();
                customers.Add(newCustomer);
            }

            Random rand = new Random();

            foreach (var concert in concerts)
            {
                for (int i = 1; i <= 100; i++)
                {
                    bool isPurchased = rand.Next(1, 101) <= 90;
                    int? randomCustomerId = null;
                    if (isPurchased)
                    {
                        int customerIndex = rand.Next(customers.Count);
                        randomCustomerId = customers[customerIndex].CustomerId;
                    }

                    var ticket = new Ticket
                    {
                        ConcertId = concert.ConcertId,
                        CustomerId = randomCustomerId,
                        Status = isPurchased ? TicketStatus.Purchased : TicketStatus.NotPurchased,
                        RNumber = rand.Next(1, 21),
                        SeatNumber = rand.Next(1, 21),
                        Price = rand.Next(300, 2501)
                    };

                    _context.Tickets.Add(ticket);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}