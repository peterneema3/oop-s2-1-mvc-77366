using Library.MVC.Data;
using Library.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bogus; 

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Add services
// -------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add Identity with Roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// -------------------------
// Ensure DB & seed data
// -------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    // Apply pending migrations
    await db.Database.MigrateAsync();

    // Seed admin user & role
    await SeedRolesAndAdminAsync(services);

    // Seed fake Books, Members, Loans
    await SeedFakeDataAsync(db);
    await SeedFakeLoansAsync(db);
}

// -------------------------
// Configure middleware
// -------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// -------------------------
// Area & default routes
// -------------------------
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Roles}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Books}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();


// -------------------------
// Seed Admin
// -------------------------
async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string adminRole = "Admin";
    string adminEmail = "admin@library.com";
    string adminPassword = "Admin123!";

    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, adminPassword);
        await userManager.AddToRoleAsync(adminUser, adminRole);
    }
}

// -------------------------
// Seed fake Books & Members
// -------------------------
async Task SeedFakeDataAsync(ApplicationDbContext db)
{
    if (!db.Books.Any())
    {
        var categories = new[] { "Fiction", "Non-Fiction", "Science", "History", "Biography" };
        var faker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.Isbn, f => f.Random.Replace("###-##########"))
            .RuleFor(b => b.Category, f => f.PickRandom(categories))
            .RuleFor(b => b.IsAvailable, f => true);

        db.Books.AddRange(faker.Generate(20));
    }

    if (!db.Members.Any())
    {
        var faker = new Faker<Member>()
            .RuleFor(m => m.FullName, f => f.Name.FullName())
            .RuleFor(m => m.Email, f => f.Internet.Email())
            .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber());

        db.Members.AddRange(faker.Generate(10));
    }

    await db.SaveChangesAsync();
}

// -------------------------
// Seed fake Loans
// -------------------------
async Task SeedFakeLoansAsync(ApplicationDbContext db)
{
    if (!db.Loans.Any())
    {
        var random = new Random();
        var books = db.Books.ToList();
        var members = db.Members.ToList();
        var loans = new List<Loan>();

        for (int i = 0; i < 15; i++)
        {
            var member = members[random.Next(members.Count)];
            var book = books[random.Next(books.Count)];

            // Skip books already on active loan
            if (loans.Any(l => l.BookId == book.Id && l.ReturnedDate == null))
                continue;

            var loanDate = DateTime.Now.AddDays(-random.Next(1, 30));
            DateTime? returnedDate = null;

            // Randomly mark some loans as returned
            if (random.NextDouble() < 0.5)
            {
                returnedDate = loanDate.AddDays(random.Next(1, 14));
                book.IsAvailable = true;
            }
            else
            {
                book.IsAvailable = false;
            }

            loans.Add(new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = loanDate,
                DueDate = loanDate.AddDays(14),
                ReturnedDate = returnedDate
            });
        }

        db.Loans.AddRange(loans);
        await db.SaveChangesAsync();
    }
}