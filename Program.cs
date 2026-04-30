using FantasyPitchXI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// db connection
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("FantasyPitchConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Player.Any())
    {
        var importer = new FantasyPitchXI.Data.Seed.PlayerCSVImporter(db);
        importer.import("Data/Seed/players.csv");
    }
    //Reset gameweek to GW1 on every application start
    GameState.ResetGameweek();
    //All available transfers go back to default
    var teams = await db.FantasyTeam.ToListAsync();
    foreach(var team in teams)
    {
        team.TransferAvailableThisGameweek = GameRuleConstants.MaxTransfersPerGW;
    }
    db.SaveChanges();
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseStaticFiles();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
