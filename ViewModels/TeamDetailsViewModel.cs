using FantasyPitchXI.Models;

namespace FantasyPitchXI.ViewModels
{
    public class TeamDetailsViewModel
    {
        public FantasyTeam Team { get; set; }
        public List<Player> Players { get; set; } = new();
        public List<Player> Starting { get; set; } = new();
        public List<Player> Bench { get; set; } = new();
    }
}
