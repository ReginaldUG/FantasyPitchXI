using FantasyPitchXI.Data;
using FantasyPitchXI.Services;
using FantasyPitchXI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FantasyPitchXI.Controllers
{
    public class CreateTeamController: Controller
    {
        private readonly TeamValidationService _teamservice;
        public CreateTeamController(AppDbContext db)
        {
            _teamservice = new TeamValidationService(db);
        }

        [HttpGet]
        public IActionResult CreateTeam()
        {
            return View();
        }


        [HttpPost]
        public IActionResult CreateTeam(string teamName)
        {
            var result = _teamservice.CreateTeam(teamName);

            if (!result.success)
            {
                return BadRequest(result.message);
            }

            var team = result.team;


            return RedirectToAction("SelectPlayers", new { teamId = team.Id });
        }

        [HttpGet]
        public IActionResult SelectPlayers(int teamId)
        {
            var team = _teamservice.GetTeamById(teamId);
            var players = _teamservice.GetAllPlayers();

            var vm = new SelectPlayersViewModel
            {
                TeamId = teamId,
                Budget = team.Budget,
                TeamName = team.TeamName,
                AllPlayers = players
            };
            
            return View(vm);
        }

        [HttpPost]
        public IActionResult SaveSquad(SelectPlayersViewModel vm)
        {
            var ids = vm.SelectedPlayerIds;

            var team = _teamservice.GetTeamById(vm.TeamId);

            var chosenplayers = _teamservice.GetPlayersByIds(ids);

            var result = _teamservice.AddPlayersToTeam(team, chosenplayers);

            if (!result.success)
            {
                TempData["Error"] = result.message;
                return RedirectToAction("SelectPlayers", new { teamId = vm.TeamId });
            }
            return RedirectToAction("TeamDetails", "Home", new {teamId= vm.TeamId});
        }
    }
}
