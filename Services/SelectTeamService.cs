using FantasyPitchXI.Data;
using FantasyPitchXI.Models;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using SQLitePCL;

namespace FantasyPitchXI.Services
{
    public class SelectTeamService
    {
        private readonly AppDbContext _db;
        public readonly TeamValidationService _teamValidationService;

        public SelectTeamService(AppDbContext db)
        {
            _db = db;
            _teamValidationService = new TeamValidationService(db);
        }

        
        public (bool proceed, string message) SelectStartingXI( FantasyTeam team, List<int> SelectedXIPlayersIDs, List<int>SelectedBenchPlayersIDs, int capID, int viceID)
        {
            List<Player> chosenXIPlayers = new List<Player>();
            List<Player> SelectedBenchPlayers = _teamValidationService.GetPlayersByIds(SelectedBenchPlayersIDs);
            int teamID = team.Id;

            //Check the number of players chosen
            if (SelectedXIPlayersIDs.Count != 11)
            {
                return (false, "You must select 11 Players");
            }

            //Check that the IDs exist as players in db
            foreach (int i in SelectedXIPlayersIDs)
            {
                var exists = _db.Player.FirstOrDefault(p => p.Id == i);
                if (exists == null)
                {
                    return (false, "ID does not exist");
                }
                chosenXIPlayers.Add(exists);
            }

            //Ensure players position count meets requirements
            var validatePlayersXI = ValidateNumberOfPlayersXI(chosenXIPlayers);
            if (!validatePlayersXI.proceed)
            {
                return (validatePlayersXI.proceed, validatePlayersXI.message);
            }

            //Validate the Captain and Vice Captain selections
            bool checkCapVice = ValidateCapValidateVice(team, chosenXIPlayers, capID, viceID);
            if (!checkCapVice)
            {
                return (false, "Invalid Cap Vice Selection");
            }            

            //Set the lineup and bench in db
            var update = UpdateXI(teamID, chosenXIPlayers, SelectedBenchPlayers, capID, viceID);
            if (!update)
            {
                return (false, "Error saving team");
            }
            
            //Save changes to db
            _db.SaveChanges();

            return (true, "XI Selected");

        }

        private bool UpdateXI(int teamID, List<Player> chosenXIPlayers, List<Player> SelectedBenchPlayers, int capID, int viceID)
        {
            int order = 0;
            //Assign IsStarting bool to starters in db with 0 as BenchOrder
            foreach (var p in chosenXIPlayers)
            {
                var addStart = new FantasyTeamLineup
                {
                    FantasyTeamId = teamID,
                    PlayerId = p.Id,
                    Gameweek = GameState.CurrentGameweek,
                    IsStarting = true,
                    BenchOrder = 0
                };
                
                if(capID == p.Id)
                {
                    addStart.IsCaptain = true;
                }

                if(viceID == p.Id)
                {
                    addStart.IsViceCaptain = true;
                }

                _db.FantasyTeamLineups.Add(addStart);
            }

            //Assign remaining squad to bench via their position in List            
            for (int i = 0; i < SelectedBenchPlayers.Count; i++)
            {
                order = order++;
                var addBench = new FantasyTeamLineup
                {
                    FantasyTeamId = teamID,
                    PlayerId = SelectedBenchPlayers[i].Id,
                    Gameweek = GameState.CurrentGameweek,
                    IsStarting = false,
                    BenchOrder = order
                };
                _db.FantasyTeamLineups.Add(addBench);
            }

            return true;

        }

        private (bool proceed, string message) ValidateNumberOfPlayersXI(List<Player> chosenPlayers)
        {
            var groupPlayers = chosenPlayers.GroupBy(p => p.Position).ToDictionary(g => g.Key, g => g.Count());

            int goalkeepers = groupPlayers.GetValueOrDefault(PlayerPosition.Goalkeeper, 0);
            int defenders = groupPlayers.GetValueOrDefault(PlayerPosition.Defender, 0);
            int midfielders = groupPlayers.GetValueOrDefault(PlayerPosition.Midfielder, 0);
            int strikers = groupPlayers.GetValueOrDefault(PlayerPosition.Striker, 0);

            string? message =
                goalkeepers > GameRuleConstants.MaxGoalkeeperXI ? $"You Must Select {GameRuleConstants.MaxGoalkeeperXI} Goalkeeper" :
                defenders < GameRuleConstants.MinDefendersXI ? $"Must have Min {GameRuleConstants.MinDefendersXI} Defender" :
                midfielders < GameRuleConstants.MinMidfieldersXI ? $"Must have Min {GameRuleConstants.MinMidfieldersXI} Midfielders" :
                strikers < GameRuleConstants.MinStrikersXI ? $"Must have Min {GameRuleConstants.MinStrikersXI} Strikers" :
                null;

            return message == null ? (true, "passed") : (false, message);

        }

        private bool ValidateCapValidateVice(FantasyTeam team, List<Player> chosenXIPlayers, int capID, int viceID)
        {
            //cap and vice id cannot be the same
            if (capID == viceID)
            {
                return false;
            }

            //cap and vice id and team xcannot be null
            if (team == null)
            {
                return false;
            }

            //cap and vice id must be players in team selected xi
            bool CapExist = false, ViceExist = false;

            foreach (Player player in chosenXIPlayers)
            {
                if (player.Id == capID)
                {
                    CapExist = true;
                }
                if (player.Id == viceID)
                {
                    ViceExist = true;
                }
            }

            if (!(CapExist && ViceExist))
            {
                return false;
            }

            return true;

        }



    }
}
