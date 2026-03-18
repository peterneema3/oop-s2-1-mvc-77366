using System.Linq;
using System.Threading.Tasks;
using Library.Domain;
using Library.MVC.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Library.MVC.Tests
{
    public class BookTests
    {
        private async Task<ApplicationDbContext> GetDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BookTestsDb")
                .Options;

            var db = new ApplicationDbContext(options);

            db.Books.AddRange(
                new Book { Title = "C# Programming", Author = "Alice", Isbn = "111-1111111111", Category = "Programming", IsAvailable = true },
                new Book { Title = "Java Essentials", Author = "Bob", Isbn = "222-2222222222", Category = "Programming", IsAvailable = true },
                new Book { Title = "History of Rome", Author = "Carol", Isbn = "333-3333333333", Category = "History", IsAvailable = true }
            );

            await db.SaveChangesAsync();
            return db;
        }

        [Fact]
        public async Task SearchReturnsExpectedMatches()
        {
            var db = await GetDbContextAsync();

            var searchTerm = "C#";
            var results = db.Books.Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm)).ToList();

            Assert.Single(results);
            Assert.Equal("C# Programming", results.First().Title);
        }
    }
}