using FantasyPitchXI.Data;
using FantasyPitchXI.Models;
using FantasyPitchXI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FantasyPitchXI.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var vm = new HomeViewModel
            {
                CurrentGameweek = GameState.CurrentGameweek
            };
            return View(vm);
        }

        public IActionResult ViewTeams()
        {
            var listOfTeams = _db.FantasyTeam.Include(t => t.FantasyTeamPlayers).ToList();

            var vm = new ViewTeamsViewModel
            {
                Teams = listOfTeams
            };
            return View(vm);
        }

        public IActionResult TeamDetails(int teamId)
        {
            var team = _db.FantasyTeam.Include(t => t.FantasyTeamPlayers).ThenInclude(tp => tp.Player).ThenInclude(p => p.Club).FirstOrDefault(t => t.Id == teamId);

            if (team == null)
            {
                return View("Error");
            }

            var vm = new TeamDetailsViewModel
            {
                Team = team,
                Players = team.FantasyTeamPlayers.Select(tp => tp.Player).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AdvanceGameweek()
        {
            if (GameState.CurrentGameweek >= GameRuleConstants.TotalGameweeks)
            {
                TempData["Error"] = "Season Over, Gameweek 38 reached";
                return RedirectToAction("Index");
            }
            GameState.CurrentGameweek++;
            var teams = await _db.FantasyTeam.ToListAsync();
            foreach(var team in teams)
            {
                if(team.TransferAvailableThisGameweek < 5)
                {
                    team.TransferAvailableThisGameweek++;
                }
                
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
