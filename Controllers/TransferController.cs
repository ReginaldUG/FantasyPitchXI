using FantasyPitchXI.Data;
using FantasyPitchXI.Services;
using FantasyPitchXI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FantasyPitchXI.Controllers
{
    public class TransferController : Controller
    {

        private readonly TransferService _transferservice;
        private readonly TeamValidationService _teamValidationService;
        private readonly AppDbContext _db;
        public TransferController(AppDbContext db)
        {
            _db = db;
            _teamValidationService = new TeamValidationService(db);
            _transferservice = new TransferService(db);

        }

        [HttpGet]
        public IActionResult Index(int teamId)
        {

            var team = _teamValidationService.GetTeamById(teamId);

            var players = _teamValidationService.GetAllPlayers();

            var currentPlayerIds = team.FantasyTeamPlayers.Select(tp => tp.PlayerId).ToList();
            var availablePlayers = _teamValidationService.GetAllPlayers().Where(p => !currentPlayerIds.Contains(p.Id)).ToList();

            var currentSquad = team.FantasyTeamPlayers.Select(tp => tp.Player).ToList();
            var vm = new TransferPlayersViewModel
            {
                TeamId = team.Id,
                TeamName = team.TeamName,
                Budget = team.Budget,
                TransfersAvailable = team.TransferAvailableThisGameweek,
                AllPlayers = availablePlayers,
                CurrentSquad = currentSquad
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult TransferAction(int teamId, List<int> updatedSquadPlayerIDs)
        {
            var team = _teamValidationService.GetTeamById(teamId);

            var result = _transferservice.TransferPlayers(team, updatedSquadPlayerIDs);

            if (!result.proceed)
            {
                return BadRequest(result.message);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
