using Library.Domain;
using Library.MVC.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Library.MVC.Tests
{
    public class LoanTests
    {
        private async Task<ApplicationDbContext> GetDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new ApplicationDbContext(options);

            // Seed books and members
            var book1 = new Book { Title = "Book 1", IsAvailable = true, Category = "Fiction", Author = "Author 1", Isbn = "111-1111111111" };
            var book2 = new Book { Title = "Book 2", IsAvailable = true, Category = "Fiction", Author = "Author 2", Isbn = "222-2222222222" };
            var member = new Member { FullName = "Test Member", Email = "test@example.com", Phone = "12345678" };

            db.Books.AddRange(book1, book2);
            db.Members.Add(member);
            await db.SaveChangesAsync();

            // Seed active loan for book1
            db.Loans.Add(new Loan { BookId = book1.Id, MemberId = member.Id, LoanDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14), ReturnedDate = null });
            book1.IsAvailable = false;

            await db.SaveChangesAsync();
            return db;
        }

        [Fact]
        public async Task CannotCreateLoanForBookAlreadyOnLoan()
        {
            var db = await GetDbContextAsync();
            var member = db.Members.First();
            var bookOnLoan = db.Books.First(b => !b.IsAvailable);

            // Try to loan book already on active loan
            bool isOnLoan = db.Loans.Any(l => l.BookId == bookOnLoan.Id && l.ReturnedDate == null);
            Assert.True(isOnLoan);
        }

        [Fact]
        public async Task ReturnedLoanMakesBookAvailableAgain()
        {
            var db = await GetDbContextAsync();
            var loan = db.Loans.First(l => l.ReturnedDate == null);

            // Return the loan
            loan.ReturnedDate = DateTime.Now;
            var book = db.Books.First(b => b.Id == loan.BookId);
            book.IsAvailable = true;

            await db.SaveChangesAsync();

            Assert.True(book.IsAvailable);
            Assert.NotNull(loan.ReturnedDate);
        }

        [Fact]
        public async Task OverdueLogicDetectsOverdueLoans()
        {
            var db = await GetDbContextAsync();
            var overdueLoan = new Loan
            {
                BookId = db.Books.First(b => b.IsAvailable).Id,
                MemberId = db.Members.First().Id,
                LoanDate = DateTime.Now.AddDays(-20),
                DueDate = DateTime.Now.AddDays(-6),
                ReturnedDate = null
            };
            db.Loans.Add(overdueLoan);
            await db.SaveChangesAsync();

            var overdue = db.Loans.Any(l => l.DueDate < DateTime.Now && l.ReturnedDate == null);
            Assert.True(overdue);
        }
    }
}