using FantasyPitchXI.Data;
using FantasyPitchXI.Models;

namespace FantasyPitchXI.Services
{
    public class TransferService
    {
        private readonly AppDbContext _db;
        public TransferService(AppDbContext db)
        {
            _db = db;
        }

        public (bool proceed, string message) TransferPlayers (FantasyTeam team, List<int> updatedSquadPlayerIDs)
        {
            var teamValidationService = new TeamValidationService(_db);
            //testing

            List<int> currentPlayersIDs = team.FantasyTeamPlayers.Select(p => p.PlayerId).ToList();

            // compare lists
            var playersOutIDs = currentPlayersIDs.Except(updatedSquadPlayerIDs).ToList();
            var playersInIDs = updatedSquadPlayerIDs.Except(currentPlayersIDs).ToList();

            int transferCount = playersOutIDs.Count();

            if(transferCount > team.TransferAvailableThisGameweek)
            {
                return (false, "You do not have enough transfers available");
            }

            //  Validate the updated team
            List<Player> updatedSquad = _db.Player.Where(p => updatedSquadPlayerIDs.Contains(p.Id)).ToList();

            var teamValidator = teamValidationService.TeamValidator(updatedSquad);

            if (!teamValidator.proceed)
            {
                return (false, teamValidator.message);
            }

            //  Update the Team in db
            //Remove players out
            foreach (var playerID in playersOutIDs)
            {
                var removePlayer = team.FantasyTeamPlayers.FirstOrDefault(p => p.PlayerId == playerID);
                if (removePlayer != null)
                {
                    team.FantasyTeamPlayers.Remove(removePlayer);
                }
            }

            //Add new players
            foreach (var playerID in playersInIDs)
            {
                var addLink = new FantasyTeamPlayer
                {
                    PlayerId = playerID,
                    FantasyTeamId = team.Id
                };
                _db.FantasyTeamPlayer.Add(addLink);
            }

            //Update team budget            
            var updatedBudget = teamValidationService.CalculateRemainingBudget(updatedSquad);
            team.Budget = updatedBudget;
            team.TransferAvailableThisGameweek -= transferCount;

            _db.SaveChanges();

            return (true, "Team Updated");
        }
    }
}
