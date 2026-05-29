using FantasyPitchXI.Data;
using FantasyPitchXI.DTO;
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

        public ApiResponse<FantasyTeam> GetTeamDetails(int teamId)
        {            
            var team = _db.FantasyTeam
                .Include(t => t.FantasyTeamPlayers)
                .ThenInclude(tp => tp.Player)
                .ThenInclude(p => p.Club)
                .FirstOrDefault(t => t.Id == teamId);          

            if (team==null)
            {
                return ApiResponse<FantasyTeam>.Fail("Team not found");
            }

            return ApiResponse<FantasyTeam>.Pass("Team retrieved successfully", team);
        }

        public ApiResponse<List<LineupPlayerDTO>> GetTeamStartingPlayers(int teamId)
        {
            try
            {
                var gw = GameState.CurrentGameweek;
                
                var starting = _db.FantasyTeamLineups
                    .Where(t => t.FantasyTeamId == teamId && t.Gameweek == gw && t.IsStarting == true)
                    .OrderBy(t=>t.FantasyTeamPlayer.Player.Position)
                    .Select(t=>new LineupPlayerDTO
                    {
                        Player = t.FantasyTeamPlayer.Player,
                        IsCaptain = t.IsCaptain,
                        IsViceCaptain = t.IsViceCaptain
                    })
                    .ToList();

                return ApiResponse<List<LineupPlayerDTO>>.Pass("proceed", starting);
            }
            catch (Exception e)
            {                
                Console.WriteLine(e);
                return ApiResponse<List<LineupPlayerDTO>>.Fail("Failed to fetch starting players");
            }           
        }

        public ApiResponse<List<Player>> GetTeamBenchPlayers(int teamId)
        {
            try
            {
                var bench = _db.FantasyTeamLineups
                    .Where(t =>
                        t.FantasyTeamId == teamId &&
                        t.Gameweek == GameState.CurrentGameweek &&
                        t.IsStarting == false
                    )
                    .OrderBy(p=>p.BenchOrder)
                    .Select(tp => tp.FantasyTeamPlayer.Player)
                    .ToList();

                return ApiResponse<List<Player>>.Pass("proceed", bench);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return ApiResponse<List<Player>>.Fail("Failed to fetch bench players");
            }
        }

        public List<Player> GetPlayersFromTeam(FantasyTeam team)
        {
            var players = team.FantasyTeamPlayers.Select(tp => tp.Player).ToList();
            return players;
        }

        public async Task<ApiResponse> AdvanceGameweek()
        {
            if(GameState.CurrentGameweek >= GameRuleConstants.TotalGameweeks)
            {
                return ApiResponse.Fail("Season Over, Gameweek 38 reached");
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

            return ApiResponse.Pass("passed");
        }
    }
}
