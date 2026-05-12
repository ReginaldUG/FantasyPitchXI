using FantasyPitchXI.Data;
using FantasyPitchXI.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyPitchXI.Services
{
    public class HomePageService
    {
        private readonly AppDbContext _db;

        public HomePageService (AppDbContext db)
        {
            _db = db;
        }

        public List<FantasyTeam> GetListOfTeams()
        {
            var fantasyTeamsList = _db.FantasyTeam.Include(t => t.FantasyTeamPlayers).ToList();
            return fantasyTeamsList;
        }

        public (bool proceed, FantasyTeam? team) GetTeamDetails(int teamId)
        {
            bool proceed = false;
            
            var team = _db.FantasyTeam.Include(t => t.FantasyTeamPlayers).ThenInclude(tp => tp.Player).ThenInclude(p => p.Club).FirstOrDefault(t => t.Id == teamId);
            

            if (team==null)
            {
                return (proceed, team);
            }

            return (true, team);
        }

        public List<Player> GetPlayersFromTeam(FantasyTeam team)
        {
            var players = team.FantasyTeamPlayers.Select(tp => tp.Player).ToList();
            return players;
        }

        public async Task<(bool proceed, string message)> AdvanceGameweek()
        {
            bool proceed = false;
            if(GameState.CurrentGameweek >= GameRuleConstants.TotalGameweeks)
            {
                return (proceed, "Season Over, Gameweek 38 reached");
            }
            GameState.CurrentGameweek++;
            var teams = await _db.FantasyTeam.ToListAsync();
            foreach(var team in teams)
            {
                if(team.TransferAvailableThisGameweek < 5)
                {
                    team.TransferAvailableThisGameweek++;
                }
            }
            await _db.SaveChangesAsync();

            return (true, "passed");
        }
    }
}
