using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Concert.Models;

namespace Concert.Controllers
{
    public class ConcertsController : Controller
    {
        private readonly ConcertContext _context;

        public ConcertsController(ConcertContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var concertContext = _context.Concerts
                .Include(c => c.Venue)
                .Include(c => c.Groups)
            .ThenInclude(g => g.Members);


            return View(await concertContext.ToListAsync());
        }
        public IActionResult RevenueChart()
        {
            return View();
        }

        // GET: Concerts/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concert = await _context.Concerts
                .Include(c => c.Venue)
                .Include(c => c.Groups) 
            .FirstOrDefaultAsync(m => m.ConcertId == id);

            if (concert == null)
            {
                return NotFound();
            }

            return View(concert);
        }


        // GET: Concerts/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name");
          
            ViewData["AllGroups"] = new SelectList(_context.Groups, "GroupId", "Name");
            return View();
        }

        // POST: Concerts/Create
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("ConcertId,Title,DateTime,VenueId,ImageUrl")] Models.Concert concert, int[] selectedGroups)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Venue");
            ModelState.Remove("Groups");
            if (concert.DateTime.HasValue)
            {
                bool hasConcertInThisVenue = _context.Concerts
                    .Any(c => c.DateTime.HasValue &&
                              c.DateTime.Value.Date == concert.DateTime.Value.Date &&
                              c.VenueId == concert.VenueId);

                if (hasConcertInThisVenue)
                {
                    ModelState.AddModelError("DateTime", "У цьому залі на цю дату вже заплановано інший концерт!");
                }
            }

            if (ModelState.IsValid)
            {
               
                if (selectedGroups != null && selectedGroups.Length > 0)
                {
                    
                    var groupsToAdd = await _context.Groups
                        .Where(g => selectedGroups.Contains(g.GroupId))
                        .ToListAsync();

                    foreach (var group in groupsToAdd)
                    {
                        concert.Groups.Add(group);
                    }
                }

                _context.Add(concert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

           
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", concert.VenueId);
            ViewData["AllGroups"] = new SelectList(_context.Groups, "GroupId", "Name");
            return View(concert);
        }

        // GET: Concerts/Edit/5
        // GET: Concerts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concert = await _context.Concerts
                .Include(c => c.Groups)
                .FirstOrDefaultAsync(m => m.ConcertId == id);

            if (concert == null)
            {
                return NotFound();
            }

       
            var selectedGroups = concert.Groups.Select(g => g.GroupId).ToList();

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", concert.VenueId);

          
            ViewData["AllGroups"] = new MultiSelectList(_context.Groups, "GroupId", "Name", selectedGroups);

            return View(concert);
        }
        // POST: Concerts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Concerts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ConcertId,Title,DateTime,VenueId,ImageUrl")] Models.Concert concert, int[] selectedGroups)
        {
            if (id != concert.ConcertId)
            {
                return NotFound();
            }

            ModelState.Remove("Venue");
            ModelState.Remove("Tickets");
            ModelState.Remove("Groups");
            if (concert.DateTime.HasValue)
            {
                bool hasConcertInThisVenue = _context.Concerts
                    .Any(c => c.DateTime.HasValue &&
                              c.DateTime.Value.Date == concert.DateTime.Value.Date &&
                              c.VenueId == concert.VenueId &&
                              c.ConcertId != concert.ConcertId);

                if (hasConcertInThisVenue)
                {
                    ModelState.AddModelError("DateTime", "У цьому залі на цю дату вже заплановано інший концерт!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                   
                    var concertToUpdate = await _context.Concerts
                        .Include(c => c.Groups)
                        .FirstOrDefaultAsync(c => c.ConcertId == id);

                    if (concertToUpdate == null) return NotFound();

               
                    concertToUpdate.Title = concert.Title;
                    concertToUpdate.DateTime = concert.DateTime;
                    concertToUpdate.VenueId = concert.VenueId;
                    concertToUpdate.ImageUrl = concert.ImageUrl;

             
                    concertToUpdate.Groups.Clear();

                
                    if (selectedGroups != null && selectedGroups.Length > 0)
                    {
                        var groupsToAdd = await _context.Groups
                            .Where(g => selectedGroups.Contains(g.GroupId))
                            .ToListAsync();

                        foreach (var group in groupsToAdd)
                        {
                            concertToUpdate.Groups.Add(group);
                        }
                    }

                    
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConcertExists(concert.ConcertId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", concert.VenueId);
            ViewData["AllGroups"] = new MultiSelectList(_context.Groups, "GroupId", "Name", selectedGroups);
            return View(concert);
        }
        // GET: Concerts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var concert = await _context.Concerts
                .Include(c => c.Venue)
                .FirstOrDefaultAsync(m => m.ConcertId == id);
            if (concert == null)
            {
                return NotFound();
            }

            return View(concert);
        }

        // POST: Concerts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var concert = await _context.Concerts.FindAsync(id);
            if (concert != null)
            {
                _context.Concerts.Remove(concert);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConcertExists(int id)
        {
            return _context.Concerts.Any(e => e.ConcertId == id);
        }
    }
}
