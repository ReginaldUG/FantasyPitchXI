using FantasyPitchXI.Data;
using FantasyPitchXI.DTO;
using FantasyPitchXI.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FantasyPitchXI.Services
{
    public class TeamValidationService
    {
        private readonly AppDbContext _db;
        public TeamValidationService(AppDbContext db)
        {
            _db = db;
        }

        //  Create a new Team
        public ApiResponse<FantasyTeam> CreateTeam(string teamName)
        {

            teamName = teamName.Trim();
            var teamExists = _db.FantasyTeam.FirstOrDefault(t => t.TeamName.ToLower() == teamName.ToLower());

            if (teamExists != null)
            {
                return ApiResponse<FantasyTeam>.Fail("Team with same name already exists");
            }

            var team = new FantasyTeam
            {
                TeamName = teamName,
                Budget = GameRuleConstants.InitialBudget,
                TransferAvailableThisGameweek = GameRuleConstants.MaxTransfersPerGW
            };
            _db.FantasyTeam.Add(team);
            _db.SaveChanges();

            return ApiResponse<FantasyTeam>.Pass("Team Created Successfully", team);
        }        

        public ApiResponse AddPlayersToTeam(FantasyTeam team, List<Player> chosenPlayers)
        {
            var teamValidator = TeamValidator(chosenPlayers);

            if (!teamValidator.Success)
            {
                return ApiResponse.Fail(teamValidator.Message);
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

            return ApiResponse.Pass("Players added successfully");
        }

        public decimal CalculateRemainingBudget(List<Player> chosenPlayers)
        {
            decimal teamCost = chosenPlayers.Sum(p => p.Price);
            decimal remainingBudget = GameRuleConstants.InitialBudget - teamCost;

            return remainingBudget;
        }

        public ApiResponse TeamValidator(List<Player> chosenPlayers)
        {
            var teamCostValidation = ValidateTeamCost(chosenPlayers);
            var numberOfPlayersValidation = ValidateNumberOfPlayers(chosenPlayers);
            var teamOfPlayersValidation = ValidateTeamOfPlayers(chosenPlayers);


            //String tenary conditional operator USED instead of nested if statement
            string? message =
                chosenPlayers.Count != GameRuleConstants.TotalSquadSize ? $"Total Player must be {GameRuleConstants.TotalSquadSize.ToString()}" :
                !teamCostValidation.Success ? teamCostValidation.Message :
                !numberOfPlayersValidation.Success ? numberOfPlayersValidation.Message :
                !teamOfPlayersValidation.Success ? teamOfPlayersValidation.Message : null;

            return message == null ? ApiResponse.Pass("Team is valid") : ApiResponse.Fail(message);
        }

        /// Validation Logic
        private ApiResponse ValidateNumberOfPlayers(List<Player> chosenPlayers)
        {
            var playerGroup = groupPlayerByPosition(chosenPlayers);

            string? message =
                playerGroup.GK != GameRuleConstants.TotalSquadGoalkeepers ? $"You Must Select {GameRuleConstants.TotalSquadGoalkeepers} Goalkeepers" :
                playerGroup.DEF != GameRuleConstants.TotalSquadDefenders ? $"You Must Select {GameRuleConstants.TotalSquadDefenders} Defenders" :
                playerGroup.MID != GameRuleConstants.TotalSquadMidfielders ? $"You Must Select {GameRuleConstants.TotalSquadMidfielders} Midfielders" :
                playerGroup.ST != GameRuleConstants.TotalSquadStrikers ? $"You Must Select {GameRuleConstants.TotalSquadStrikers} Strikers" :
                null;

            return message == null ? ApiResponse.Pass("passed") : ApiResponse.Fail(message);

        }

        public PlayerGroupingDTO groupPlayerByPosition(List<Player> playerTogroup)
        {
            var groupPlayers = playerTogroup.GroupBy(p => p.Position).ToDictionary(g => g.Key, g => g.Count());


            int GK = groupPlayers.GetValueOrDefault(PlayerPosition.Goalkeeper, 0);
            int DEF = groupPlayers.GetValueOrDefault(PlayerPosition.Defender, 0);
            int MID = groupPlayers.GetValueOrDefault(PlayerPosition.Midfielder, 0);
            int ST = groupPlayers.GetValueOrDefault(PlayerPosition.Striker, 0);


            return new PlayerGroupingDTO
            {
                GK = GK,
                DEF = DEF,
                MID = MID,
                ST = ST,
            };
        }

        private ApiResponse ValidateTeamOfPlayers(List<Player> chosenPlayers)
        {
            string message = "passed";

            var groupClubs = chosenPlayers.GroupBy(p => p.ClubId).ToDictionary(g => g.Key, g=> g.Count());
            foreach (var club in groupClubs)
            {
                if (club.Value > GameRuleConstants.MaxPlayersFromSameClub)
                {
                    message = $"You cannot select more than {GameRuleConstants.MaxPlayersFromSameClub} players from the same club.";

                    return ApiResponse.Fail(message);
                }
            }
            return ApiResponse.Pass(message);
        }

        private ApiResponse ValidateTeamCost(List<Player> chosenPlayers)
        {
            string message = "passed";
            decimal teamCost = chosenPlayers.Sum(p => p.Price);

            if (teamCost > GameRuleConstants.InitialBudget)
            {

                message = $"Team cost exceeds Max Budget of {GameRuleConstants.InitialBudget.ToString()}m";

                return ApiResponse.Fail(message);
            }
            return ApiResponse.Pass(message);
        }

        //General
        public List<Player> GetAllPlayers()
        {
            var result = _db.Player.Include(p => p.Club).ToList();
            return result;
        }
        public FantasyTeam GetTeamById(int teamId)
        {
            var result = _db.FantasyTeam.Include(t => t.FantasyTeamPlayers).ThenInclude(tp => tp.Player).ThenInclude(p => p.Club).FirstOrDefault(t => t.Id == teamId);
            return result;
        }
        public List<Player> GetPlayersByIds(List<int> playerIds)
        {
            var result = _db.Player.Include(p => p.Club).Where(p => playerIds.Contains(p.Id)).ToList();
            return result;
        }
    }
}