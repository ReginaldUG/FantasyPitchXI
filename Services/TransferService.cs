using FantasyPitchXI.Data;
using FantasyPitchXI.DTO;
using FantasyPitchXI.DTO.Request_DTO;
using FantasyPitchXI.Models;

namespace FantasyPitchXI.Services
{
    public class TransferService
    {
        private readonly AppDbContext _db;
        private readonly TeamValidationService _teamValidationService;
        public TransferService(AppDbContext db)
        {
            _db = db;
            _teamValidationService = new TeamValidationService(db);
        }

        //adjusted
        public TransferViewDTO GetTransferControllerVMProperties(int teamId)
        {
            var team = _teamValidationService.GetTeamById(teamId);
            var currentPlayerIds = team.FantasyTeamPlayers.Select(tp => tp.PlayerId).ToList();
            var availablePlayers = _teamValidationService.GetAllPlayers().Where(p => !currentPlayerIds.Contains(p.Id)).ToList();
            var currentSquad = team.FantasyTeamPlayers.Select(tp => tp.Player).ToList();

            return new TransferViewDTO {
                Team = team, 
                AvailablePlayers = availablePlayers, 
                CurrentSquad = currentSquad 
            };
        }

        public ApiResponse TransferPlayers(TransferPlayersRequestDTO request)
        {

            List<int> currentPlayersIDs = request.Team.FantasyTeamPlayers.Select(p => p.PlayerId).ToList();

            // compare lists
            var playersOutIDs = currentPlayersIDs.Except(request.UpdatedSquadPlayerIDs).ToList();
            var playersInIDs = request.UpdatedSquadPlayerIDs.Except(currentPlayersIDs).ToList();

            int transferCount = playersOutIDs.Count();

            if (transferCount > request.Team.TransferAvailableThisGameweek)
            {
                return ApiResponse.Fail("You do not have enough transfers available");
            }

            //  Validate the updated team
            List<Player> updatedSquad = _db.Player.Where(p => request.UpdatedSquadPlayerIDs.Contains(p.Id)).ToList();
            var validate = _teamValidationService.TeamValidator(updatedSquad);
            if (!validate.Success)
            {
                return ApiResponse.Fail(validate.Message);
            }

            var response = new UpdateFantasyTeamRequestDTO
            {
                Team = request.Team,
                PlayersOutIDs = playersOutIDs,
                PlayersInIDs = playersInIDs,
                UpdatedSquad = updatedSquad,
                TransferCount = transferCount
            };

            //  Update the Team in db
            UpdateFantasyTeam(response);

            return ApiResponse.Pass("Team Updated");
        }


        private bool UpdateFantasyTeam(UpdateFantasyTeamRequestDTO request)
        {
            //remove players out
            foreach (var playerID in request.PlayersOutIDs)
            {
                var removePlayer = request.Team.FantasyTeamPlayers.FirstOrDefault(p => p.PlayerId == playerID);
                if (removePlayer != null)
                {
                    request.Team.FantasyTeamPlayers.Remove(removePlayer);
                }
            }
            //add players in
            foreach (var playerID in request.PlayersInIDs)
            {
                var addLink = new FantasyTeamPlayer
                {
                    PlayerId = playerID,
                    FantasyTeamId = request.Team.Id
                };
                _db.FantasyTeamPlayer.Add(addLink);
            }
            //update budget
            var updateBudget = _teamValidationService.CalculateRemainingBudget(request.UpdatedSquad);
            request.Team.Budget = updateBudget;
            request.Team.TransferAvailableThisGameweek -= request.TransferCount;

            _db.SaveChanges();

            return true;
        }
    }
}
