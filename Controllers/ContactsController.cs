using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Authorization;

namespace CRM.Controllers
{
    public class ContactsController : Controller
    {
        private readonly CRMContext _context;

        public ContactsController(CRMContext context)
        {
            _context = context;
        }

        // ---------------- INDEX ----------------
        [Authorize]
        public async Task<IActionResult> Index(string filter)
        {
            var user = await _context.User
                   .FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);

            ViewBag.userId = user.Id;

            ViewBag.data = _context.Company
                .Where(c => c.IsDeleted == 0)
                .ToList();

            ViewBag.data2 = _context.User.ToList();

            var qry = _context.Contact
                .Where(c => c.IsDeleted == 0)
                .OrderBy(c => c.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
                qry = qry.Where(c => c.Surname.Contains(filter));

            ViewBag.filter = filter;
            return View(await qry.ToListAsync());
        }

        // ---------------- DETAILS ----------------
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contact = await _context.Contact.FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null) return NotFound();

            ViewBag.data = _context.Company.Where(c => c.IsDeleted == 0).ToList();
            ViewBag.data2 = _context.User.ToList();

            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            ViewBag.userId = user.Id;

            return View(contact);
        }

        // ---------------- CREATE (GET) ----------------
        [Authorize]
        public async Task<IActionResult> CreateAsync(int? com)
        {
            var loggedUser = await _context.User
                .FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);

            ViewBag.userId = loggedUser.Id;

            var companies = _context.Company.Where(c => c.IsDeleted == 0).ToList();
            ViewBag.data = companies;

            if (com != null)
            {
                var selectedCompany = companies.FirstOrDefault(c => c.Id == com);
                if (selectedCompany != null)
                {
                    ViewBag.companyId = com;
                }
            }

            return View();
        }

        // ---------------- CREATE (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact contact)
        {
            if (contact.UserId == 0)
            {
                var loggedUser = await _context.User
                    .FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
                contact.UserId = loggedUser.Id;
            }

            if (ModelState.IsValid)
            {
                contact.IsDeleted = 0;
                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.data = _context.Company.Where(c => c.IsDeleted == 0).ToList();
            return View(contact);
        }

        // ---------------- EDIT (GET) ----------------
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contact = await _context.Contact.FindAsync(id);
            if (contact == null) return NotFound();

            var loggedUser = await _context.User
                 .FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);

            if (loggedUser.Id != contact.UserId)
                return Unauthorized();

            ViewBag.data = _context.Company.Where(c => c.IsDeleted == 0).ToList();

            return View(contact);
        }

        // ---------------- EDIT (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contact contact)
        {
            if (id != contact.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Contact.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.data = _context.Company.Where(c => c.IsDeleted == 0).ToList();
            return View(contact);
        }

        // ---------------- DELETE (GET) ----------------
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contact = await _context.Contact.FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null) return NotFound();

            var loggedUser = await _context.User
                  .FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);

            if (loggedUser.Id != contact.UserId)
                return Unauthorized();

            ViewBag.data = _context.Company.Where(c => c.IsDeleted == 0).ToList();
            ViewBag.data2 = _context.User.ToList();

            return View(contact);
        }

        // ---------------- DELETE (POST) ----------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contact.FindAsync(id);
            contact.IsDeleted = 1;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
