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
    // ЗАХИСТ: Доступ до цієї сторінки має ТІЛЬКИ Адміністратор
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly ConcertContext _context;

        public CustomersController(ConcertContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ДОДАНО: Password та IsAdmin, щоб їх можна було зберегти при створенні
        public async Task<IActionResult> Create([Bind("CustomerId,FullName,BirthDate,LoyaltyDiscount,Email,Password,IsAdmin")] Customer customer)
        {
            ModelState.Remove("Tickets");

            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
     
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,BirthDate,LoyaltyDiscount,Email,IsAdmin")] Customer customer)
        {
            if (id != customer.CustomerId) return NotFound();

            ModelState.Remove("Tickets");
            ModelState.Remove("Password"); 

            if (ModelState.IsValid)
            {
                try
                {
                
                    var existingCustomer = await _context.Customers.FindAsync(id);
                    if (existingCustomer == null) return NotFound();

                    // Оновлюємо ТІЛЬКИ ті поля, що прийшли з форми
                    existingCustomer.FullName = customer.FullName;
                    existingCustomer.BirthDate = customer.BirthDate;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.LoyaltyDiscount = customer.LoyaltyDiscount;
                    existingCustomer.IsAdmin = customer.IsAdmin;

                    // Зберігаємо зміни
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}