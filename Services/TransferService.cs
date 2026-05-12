using FantasyPitchXI.Data;
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
        public (FantasyTeam team, List<Player> availablePlayers, List<Player> currentSquad) GetTransferControllerVMProperties(int teamId)
        {
            var team = _teamValidationService.GetTeamById(teamId);
            var currentPlayerIds = team.FantasyTeamPlayers.Select(tp => tp.PlayerId).ToList();
            var availablePlayers = _teamValidationService.GetAllPlayers().Where(p => !currentPlayerIds.Contains(p.Id)).ToList();
            var currentSquad = team.FantasyTeamPlayers.Select(tp => tp.Player).ToList();

            return (team, availablePlayers, currentSquad);
        }

        public (bool proceed, string message) TransferPlayers(FantasyTeam team, List<int> updatedSquadPlayerIDs)
        {

            List<int> currentPlayersIDs = team.FantasyTeamPlayers.Select(p => p.PlayerId).ToList();

            // compare lists
            var playersOutIDs = currentPlayersIDs.Except(updatedSquadPlayerIDs).ToList();
            var playersInIDs = updatedSquadPlayerIDs.Except(currentPlayersIDs).ToList();

            int transferCount = playersOutIDs.Count();

            if (transferCount > team.TransferAvailableThisGameweek)
            {
                return (false, "You do not have enough transfers available");
            }

            //  Validate the updated team
            List<Player> updatedSquad = _db.Player.Where(p => updatedSquadPlayerIDs.Contains(p.Id)).ToList();
            var validate = _teamValidationService.TeamValidator(updatedSquad);
            if (!validate.proceed)
            {
                return (false, validate.message);
            }

            //  Update the Team in db
            UpdateFantasyTeam(team, playersOutIDs, playersInIDs, updatedSquad, transferCount);

            return (true, "Team Updated");
        }


        private bool UpdateFantasyTeam(FantasyTeam team, List<int> playersOutIDs, List<int> playersInIDs, List<Player> updatedSquad, int transferCount)
        {
            //remove players out
            foreach (var playerID in playersOutIDs)
            {
                var removePlayer = team.FantasyTeamPlayers.FirstOrDefault(p => p.PlayerId == playerID);
                if (removePlayer != null)
                {
                    team.FantasyTeamPlayers.Remove(removePlayer);
                }
            }
            //add players in
            foreach (var playerID in playersInIDs)
            {
                var addLink = new FantasyTeamPlayer
                {
                    PlayerId = playerID,
                    FantasyTeamId = team.Id
                };
                _db.FantasyTeamPlayer.Add(addLink);
            }
            //update budget
            var updateBudget = _teamValidationService.CalculateRemainingBudget(updatedSquad);
            team.Budget = updateBudget;
            team.TransferAvailableThisGameweek -= transferCount;

            _db.SaveChanges();

            return true;
        }
    }
}
