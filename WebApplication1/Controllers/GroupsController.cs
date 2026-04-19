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
    public class GroupsController : Controller
    {
        private readonly ConcertContext _context;

        public GroupsController(ConcertContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _context.Groups
                .Include(g => g.Members)
                .ToListAsync();

            return View(groups);
        }

        public IActionResult ActivityChart()
        {
            return View();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // 1. Отримуємо групу з усіма зв'язками
            var @group = await _context.Groups
                .Include(g => g.Concerts)
                    .ThenInclude(c => c.Venue)
                .Include(g => g.Members)
                    .ThenInclude(m => m.RoleNavigation)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (@group == null) return NotFound();

            // 2. Отримуємо коментарі до ВСІХ концертів, де брав участь цей гурт
            var comments = await _context.Comments
                .Include(c => c.Customer)
                .Include(c => c.Concert)
                .Where(c => c.Concert.Groups.Any(g => g.GroupId == id))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // 3. Передаємо коментарі у View через ViewBag або ViewModel
            ViewBag.GroupComments = comments;

            return View(@group);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["AllMembers"] = new SelectList(_context.Members, "MemberId", "FullName");
            ViewBag.Roles = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("GroupId,Name,Description,LogoUrl,BgVideoUrl,FanClubUrl,VideoUrl")] Group @group, int[] selectedMembers)
        {
            ModelState.Remove("Members");
            ModelState.Remove("Concerts");

            if (ModelState.IsValid)
            {
                if (selectedMembers != null && selectedMembers.Length > 0)
                {
                    var membersToAdd = await _context.Members
                        .Where(m => selectedMembers.Contains(m.MemberId))
                        .ToListAsync();

                    foreach (var member in membersToAdd)
                    {
                        @group.Members.Add(member);
                    }
                }

                _context.Add(@group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AllMembers"] = new SelectList(_context.Members, "MemberId", "FullName");
            return View(@group);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (@group == null)
            {
                return NotFound();
            }

            var selectedMembers = @group.Members.Select(m => m.MemberId).ToList();
            ViewData["AllMembers"] = new MultiSelectList(_context.Members, "MemberId", "FullName", selectedMembers);

            return View(@group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("GroupId,Name,Description,LogoUrl,BgVideoUrl,FanClubUrl,VideoUrl")] Group @group, int[] selectedMembers)
        {
            if (id != @group.GroupId)
            {
                return NotFound();
            }

            ModelState.Remove("Members");
            ModelState.Remove("Concerts");

            if (ModelState.IsValid)
            {
                try
                {
                    var groupToUpdate = await _context.Groups
                        .Include(g => g.Members)
                        .FirstOrDefaultAsync(g => g.GroupId == id);

                    if (groupToUpdate == null) return NotFound();

                    groupToUpdate.Name = @group.Name;
                    groupToUpdate.Description = @group.Description;
                    groupToUpdate.LogoUrl = @group.LogoUrl;
                    groupToUpdate.BgVideoUrl = @group.BgVideoUrl;
                    groupToUpdate.VideoUrl = @group.VideoUrl;
                    groupToUpdate.FanClubUrl = @group.FanClubUrl;

                    groupToUpdate.Members.Clear();

                    if (selectedMembers != null && selectedMembers.Length > 0)
                    {
                        var membersToAdd = await _context.Members
                            .Where(m => selectedMembers.Contains(m.MemberId))
                            .ToListAsync();

                        foreach (var member in membersToAdd)
                        {
                            groupToUpdate.Members.Add(member);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.GroupId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine("ПОМИЛКА ВАЛІДАЦІЇ: " + error.ErrorMessage);
                }
            }

            ViewData["AllMembers"] = new MultiSelectList(_context.Members, "MemberId", "FullName", selectedMembers);
            return View(@group);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .Include(g => g.Concerts)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Concerts)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (group != null)
            {
                group.Concerts.Clear();
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.GroupId == id);
        }
    }
}