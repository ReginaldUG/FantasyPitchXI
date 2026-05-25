using FantasyPitchXI.Data;
using FantasyPitchXI.Models;
using FantasyPitchXI.Services;
using FantasyPitchXI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FantasyPitchXI.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly HomePageService _homePageService;

        public HomeController(AppDbContext db)
        {
            _db = db;
            _homePageService = new HomePageService(db);
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
            var listOfTeams = _homePageService.GetListOfTeams();

            var vm = new ViewTeamsViewModel
            {
                Teams = listOfTeams
            };
            return View(vm);
        }

        public IActionResult TeamDetails(int teamId)
        {
            var props = _homePageService.GetTeamDetails(teamId);

            if (props.Success == false)
            {
                return View("Error");
            }

            var vm = new TeamDetailsViewModel
            {
                Team = props.Data,
                Players = _homePageService.GetPlayersFromTeam(props.Data),
                Starting = _homePageService.GetTeamStartingPlayers(teamId).Data,
                Bench = _homePageService.GetTeamBenchPlayers(teamId).Data                
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AdvanceGameweek()
        {
            await _homePageService.AdvanceGameweek();
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
