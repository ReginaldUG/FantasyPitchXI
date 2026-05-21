using FantasyPitchXI.Models;

namespace FantasyPitchXI.ViewModels
{
    public class SelectPlayersViewModel
    {
        public int TeamId { get; set; }
        public decimal Budget { get; set; }
        public List<Player> AllPlayers { get; set; } = new();
        public List<int> SelectedPlayerIds { get; set; } = new();
        public List<Player> SelectedPlayers { get; set; } = new();
        public string TeamName { get; set; }
    }
}
