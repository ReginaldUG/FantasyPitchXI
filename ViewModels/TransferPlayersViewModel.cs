using FantasyPitchXI.Models;

namespace FantasyPitchXI.ViewModels
{
    public class TransferPlayersViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public decimal Budget { get; set; }
        public int TransfersAvailable { get; set; }
        public List<Player> AllPlayers { get; set; } = new();
        public List<Player> CurrentSquad { get; set;  } = new();
        public List<int> UpdatedSquadPlayerIDs { get; set; } = new();

    }
}
