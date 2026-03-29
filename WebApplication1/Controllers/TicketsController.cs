using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Concert.Models;
using System.Text.Json;

namespace Concert.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ConcertContext _context;

        public TicketsController(ConcertContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Concert)
                .Include(t => t.Customer)
                .ToListAsync();

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

        // GET: Tickets/Details/5
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

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            ViewData["ConcertId"] = new SelectList(_context.Concerts, "ConcertId", "Title");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "FullName");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Tickets/Edit/5
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

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,ConcertId,CustomerId,SeatNumber,RNumber,Price,Status")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }

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

        // GET: Tickets/Delete/5
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

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
    }
}
