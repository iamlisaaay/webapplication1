using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Concert.Models;
using Microsoft.AspNetCore.Authorization;

namespace Concert.Controllers
{
    public class MembersController : Controller
    {
        private readonly ConcertContext _context;

        public MembersController(ConcertContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var concertContext = _context.Members
                .Include(m => m.RoleNavigation)
                .Include(m => m.Groups);

            return View(await concertContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.RoleNavigation)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Role"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("MemberId,FullName,Role,ImageUrl,InstagramUrl")] Member member)
        {
            ModelState.Remove("RoleNavigation");
            ModelState.Remove("Groups");

            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Role"] = new SelectList(_context.Roles, "RoleId", "RoleName", member.Role);
            return View(member);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            ViewData["Role"] = new SelectList(_context.Roles, "RoleId", "RoleName", member.Role);
            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("MemberId,FullName,Role,ImageUrl,InstagramUrl")] Member member)
        {
            ModelState.Remove("RoleNavigation");
            ModelState.Remove("Groups");
            if (id != member.MemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.MemberId))
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
            ViewData["Role"] = new SelectList(_context.Roles, "RoleId", "RoleName", member.Role);
            return View(member);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.RoleNavigation)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members
                .Include(m => m.Groups)
                    .ThenInclude(g => g.Members)
                .FirstOrDefaultAsync(m => m.MemberId == id);

            if (member != null)
            {
                foreach (var group in member.Groups.ToList())
                {
                    if (group.Members.Count <= 1)
                    {
                        _context.Groups.Remove(group);
                    }
                }

                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAjax(Member member)
        {
            ModelState.Remove("RoleNavigation");
            ModelState.Remove("Groups");

            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();

                return Json(new { success = true, id = member.MemberId, name = member.FullName });
            }

            return Json(new { success = false, message = "Помилка валідації. Перевірте введені дані." });
        }
    }
}