using FantasyPitchXI.Data;
using FantasyPitchXI.DTO.Request_DTO;
using FantasyPitchXI.Services;
using FantasyPitchXI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FantasyPitchXI.Controllers
{
    public class PickTeamController: Controller
    {
        private readonly AppDbContext _db;
        private readonly PickTeamService _pickTeamService;
        private readonly TeamValidationService _teamservice;
        private readonly HomePageService _homePageService;

        public PickTeamController(AppDbContext db)
        {
            _db = db;
            _pickTeamService = new PickTeamService(db);
            _teamservice = new TeamValidationService(db);
            _homePageService = new HomePageService(db);
        }

        [HttpGet]
        public IActionResult PickTeam(int teamId)
        {
            var team = _teamservice.GetTeamById(teamId);
            var starting = _homePageService.GetTeamStartingPlayers(teamId).Data;
            var bench = _homePageService.GetTeamBenchPlayers(teamId).Data;

            var request = new GetTeamCurrentSquadRequestDTO()
            {
                TeamID = teamId
            };
            //var squad = _pickTeamService.GetTeamCurrentSquad(request).SquadPlayers;
            bool isTeamLineupComplete = starting?.Count == 11 && bench?.Count == 4;
            var squad = isTeamLineupComplete ? [] : _pickTeamService.GetTeamCurrentSquad(request).SquadPlayers;
            
            var vm = new PickTeamViewModel
            {
                TeamId = teamId,
                TeamName = team.TeamName,
                Squad = squad,
                Starting = starting ?? [],
                Bench = bench ?? []
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult SaveTeamXI(PickTeamViewModel vm)
        {
            var capId = vm.CaptainID;
            var viceCapId = vm.ViceCaptainID;
            var team = _teamservice.GetTeamById(vm.TeamId);
            var selectedXIPlayersIds = vm.StartingTeamPlayerIDs;
            var selectedBenchPlayerIds = vm.BenchPlayerIDs;

            var request = new SelectStartingXIandBenchRequestDTO
            {
                Team = team,
                StartingIDs = selectedXIPlayersIds,
                BenchIDs = selectedBenchPlayerIds,
                CapID = capId,
                ViceID = viceCapId
            };
            var result = _pickTeamService.SelectStartingXIandBench(request);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                return RedirectToAction("PickTeam", new { teamId = vm.TeamId });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}