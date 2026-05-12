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
            //adjusted
            var props = _transferservice.GetTransferControllerVMProperties(teamId);

            var vm = new TransferPlayersViewModel
            {
                TeamId = props.team.Id,
                TeamName = props.team.TeamName,
                Budget = props.team.Budget,
                TransfersAvailable = props.team.TransferAvailableThisGameweek,
                AllPlayers = props.availablePlayers,
                CurrentSquad = props.currentSquad
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
