using FantasyPitchXI.Data;
using FantasyPitchXI.DTO.Request_DTO;
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
                TeamId = props.Team.Id,
                TeamName = props.Team.TeamName,
                Budget = props.Team.Budget,
                TransfersAvailable = props.Team.TransferAvailableThisGameweek,
                AllPlayers = props.AvailablePlayers,
                CurrentSquad = props.CurrentSquad
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult TransferAction(int teamId, List<int> updatedSquadPlayerIDs)
        {
            var team = _teamValidationService.GetTeamById(teamId);

            var request = new TransferPlayersRequestDTO
            {
                Team = team,
                UpdatedSquadPlayerIDs = updatedSquadPlayerIDs
            };

            var result = _transferservice.TransferPlayers(request);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
