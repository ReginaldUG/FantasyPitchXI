using FantasyPitchXI.Data;
using FantasyPitchXI.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyPitchXI.Services
{
    public class TeamValidationService
    {
        private readonly AppDbContext _db;
        public TeamValidationService (AppDbContext db)
        {
            _db = db;
        }

        //  Create a new Team
        public (bool success, string message, FantasyTeam? team) CreateTeam(string teamName)
        {
            teamName = teamName.Trim();
            var teamExists = _db.FantasyTeam.FirstOrDefault(t => t.TeamName.ToLower() == teamName.Trim().ToLower());
            if (teamExists!=null)
            {
                return (false, "Team with same name already exists", null);
            }
            var team = new FantasyTeam
            {
                TeamName = teamName.Trim(),
                Budget = GameRuleConstants.InitialBudget,
                TransferAvailableThisGameweek = GameRuleConstants.MaxTransfersPerGW
            };
            _db.FantasyTeam.Add(team);
            _db.SaveChanges();

            return (true, "Team Created Successfully", team);
        }

        public (bool success, string message) AddPlayersToTeam(FantasyTeam team, List<Player> chosenPlayers)
        {
            var teamValidator = TeamValidator(chosenPlayers);
            
            if (!teamValidator.proceed)
            {
                return (false, teamValidator.message);
            }

            foreach (var player in chosenPlayers)
            {
                var addLink = new FantasyTeamPlayer
                {
                    FantasyTeamId = team.Id,
                    PlayerId = player.Id
                };
                _db.FantasyTeamPlayer.Add(addLink);
            }

            //Update Team Budget. Initial Budget is always 100m
            team.Budget = CalculateRemainingBudget(chosenPlayers);

            
            _db.SaveChanges();

            return (true, "Players added successfully");
        }

        public decimal CalculateRemainingBudget(List<Player> chosenPlayers)
        {
            decimal teamCost = chosenPlayers.Sum(p => p.Price);
            decimal remainingBudget = GameRuleConstants.InitialBudget - teamCost;

            return remainingBudget;
        }

        public (bool proceed, string message) TeamValidator(List<Player> chosenPlayers)
        {
            var teamCostValidation = ValidateTeamCost(chosenPlayers);
            var numberOfPlayersValidation = ValidateNumberOfPlayers(chosenPlayers);
            var teamOfPlayersValidation = ValidateTeamOfPlayers(chosenPlayers);

            if (chosenPlayers.Count != GameRuleConstants.TotalSquadSize)
            {
                return (false, $"Total Player must be {GameRuleConstants.TotalSquadSize.ToString()}");
            }
            else if (!teamCostValidation.proceed)
            {
                return (false, teamCostValidation.message);
            }
            else if (!numberOfPlayersValidation.proceed)
            {
                return (false, numberOfPlayersValidation.message);
            }
            else if (!teamOfPlayersValidation.proceed)
            {
                return (false, teamOfPlayersValidation.message);
            }

            return (true, "Team is valid");

        }

        /// Validation Logic
        private (bool proceed, string message) ValidateNumberOfPlayers(List<Player> chosenPlayers)
        {
            bool proceed = false;
            string message = "passed";

            var groupPlayers = chosenPlayers.GroupBy(p => p.Position).ToDictionary(g => g.Key, g => g.Count());

            int goalkeepers = groupPlayers.GetValueOrDefault(PlayerPosition.Goalkeeper, 0);
            int defenders = groupPlayers.GetValueOrDefault(PlayerPosition.Defender, 0);
            int midfielders = groupPlayers.GetValueOrDefault(PlayerPosition.Midfielder, 0);
            int strikers = groupPlayers.GetValueOrDefault(PlayerPosition.Striker, 0);

            if (goalkeepers != GameRuleConstants.TotalSquadGoalkeepers)
            {
                message = $"You Must Select {GameRuleConstants.TotalSquadGoalkeepers} Goalkeepers";
            }
            else if (defenders != GameRuleConstants.TotalSquadDefenders)
            {
                message = $"You Must Select {GameRuleConstants.TotalSquadDefenders} Defenders";
            }
            else if (midfielders != GameRuleConstants.TotalSquadMidfielders)
            {
                message = $"You Must Select {GameRuleConstants.TotalSquadMidfielders} Midfielders";
            }
            else if (strikers != GameRuleConstants.TotalSquadStrikers)
            {
                message = $"You Must Select {GameRuleConstants.TotalSquadStrikers} Strikers";
            }
            else
            {
                proceed = true;
            }

            return (proceed, message);
        }

        private (bool proceed, string message) ValidateTeamOfPlayers(List<Player> chosenPlayers)
        {
            bool proceed = true;
            string message = "passed";

            var groupClubs = chosenPlayers.GroupBy(p => p.ClubId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var club in groupClubs)
            {
                if (club.Value > GameRuleConstants.MaxPlayersFromSameClub)
                {
                    message = $"You cannot select more than {GameRuleConstants.MaxPlayersFromSameClub} players from the same club.";
                    proceed = false;
                     
                    return (proceed, message);                    
                }
            }

            return (proceed, message);
        }

        private (bool proceed, string message) ValidateTeamCost(List<Player> chosenPlayers)
        {
            bool proceed = true;
            string message = "passed";

            decimal teamCost = chosenPlayers.Sum(p => p.Price);

            if (teamCost > GameRuleConstants.InitialBudget){

                message = $"Team cost exceeds Max Budget of {GameRuleConstants.InitialBudget.ToString()}m";
                proceed = false;

                return (proceed, message);
            }

            return (proceed, message);
        }

        //General
        public List<Player> GetAllPlayers()
        {
            var result = _db.Player.Include(p=>p.Club).ToList();
            return result;
        }
        public FantasyTeam GetTeamById(int teamId)
        {
            var result = _db.FantasyTeam.Include(t => t.FantasyTeamPlayers).ThenInclude(tp=>tp.Player).ThenInclude(p=>p.Club).FirstOrDefault(t => t.Id == teamId);
            return result;
        }
        public List<Player> GetPlayersByIds(List<int> playerIds)
        {
            var result = _db.Player.Include(p=>p.Club).Where(p => playerIds.Contains(p.Id)).ToList();
            return result;
        }
    }
}
