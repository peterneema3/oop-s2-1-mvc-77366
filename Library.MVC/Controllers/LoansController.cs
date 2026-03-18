using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Domain;
using Library.MVC.Data;

namespace Library.MVC.Controllers
{
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            return View(loans);
        }

        // GET: Loans/Create
        public IActionResult Create()
        {
            // Only show available books
            var availableBooks = _context.Books.Where(b => b.IsAvailable).ToList();

            ViewData["BookId"] = new SelectList(availableBooks, "Id", "Title");
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName");

            return View();
        }

        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,MemberId,DueDate")] Loan loan)
        {
            if (_context.Loans.Any(l => l.BookId == loan.BookId && l.ReturnedDate == null))
            {
                ModelState.AddModelError("", "This book is already on an active loan.");
            }

            if (ModelState.IsValid)
            {
                loan.LoanDate = DateTime.Now;

                _context.Loans.Add(loan);

                // Mark book unavailable
                var book = await _context.Books.FindAsync(loan.BookId);
                if (book != null) book.IsAvailable = false;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Rebuild dropdowns if validation fails
            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable), "Id", "Title", loan.BookId);
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "FullName", loan.MemberId);
            return View(loan);
        }

        // POST: Loans/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null) return NotFound();

            loan.ReturnedDate = DateTime.Now;

            var book = await _context.Books.FindAsync(loan.BookId);
            if (book != null) book.IsAvailable = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Loans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            return View(loan);
        }

        // POST: Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan != null)
            {
                if (loan.ReturnedDate == null)
                {
                    var book = await _context.Books.FindAsync(loan.BookId);
                    if (book != null) book.IsAvailable = true;
                }

                _context.Loans.Remove(loan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}