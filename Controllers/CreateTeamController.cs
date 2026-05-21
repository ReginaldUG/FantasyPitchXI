using FantasyPitchXI.Data;
using FantasyPitchXI.Models;
using FantasyPitchXI.Services;
using FantasyPitchXI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FantasyPitchXI.Controllers
{
    public class CreateTeamController : Controller
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

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var team = result.Data;


            return RedirectToAction("SelectPlayers", new { teamId = team.Id });
        }

        [HttpGet]
        public IActionResult SelectPlayers(int teamId, List<int>? prevSelection)
        {
            var team = _teamservice.GetTeamById(teamId);
            var allPlayers = _teamservice.GetAllPlayers();
            var selectedPlayers = new List<Player>();

            //If there are previously selected players, we need to separate them from the available players
            if (prevSelection != null && prevSelection.Any())
            {
                selectedPlayers = _teamservice.GetPlayersByIds(prevSelection);
                allPlayers = allPlayers.Where(p => !prevSelection.Contains(p.Id)).ToList();                
            }

            var vm = new SelectPlayersViewModel
            {
                TeamId = teamId,
                Budget = team.Budget,
                TeamName = team.TeamName,
                AllPlayers = allPlayers,
                SelectedPlayers = selectedPlayers
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

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("SelectPlayers", new { teamId = vm.TeamId, prevSelection = vm.SelectedPlayerIds });
            }
            return RedirectToAction("PickTeam", "PickTeam", new { teamId = vm.TeamId });
        }
    }
}
