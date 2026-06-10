using DormHub.Data;
using DormHub.Hubs;
using DormHub.Models;
using DormHub.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHttpClient<CurrencyService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "DormHub API",
        Version     = "v1",
        Description = "Szwagier DormHub'u"
    });
});

builder.Services.AddDbContext<DormDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AUTH
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "DormHubAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Apply pending migrations and ensure a default admin account exists.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DormDbContext>();
    db.Database.Migrate();

    if (!db.Persons.Any(p => p.Role == "Admin"))
    {
        db.Persons.Add(new PersonModel
        {
            FirstName = "Admin",
            LastName = "DormHub",
            Email = "admin@dormhub.local",
            PhoneNumber = "000000000",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            PasswordHash = PasswordHasher.Hash("root"),
            Role = "Admin",
            IsActive = true
        });
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DormHub API v1");
    c.DocumentTitle = "DormHub API";
    c.RoutePrefix = "swagger";
});

app.MapRazorPages();

app.MapHub<ChatHub>("/chathub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
Console.WriteLine(DormHub.Services.PasswordHasher.Hash("wsadwsad"));
app.Run();
