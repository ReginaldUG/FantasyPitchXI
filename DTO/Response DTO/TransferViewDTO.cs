using FantasyPitchXI.Models;

namespace FantasyPitchXI.DTO
{
    public class TransferViewDTO
    {
        public FantasyTeam Team { get; set; }
        public List<Player> AvailablePlayers { get; set; }
        public List<Player> CurrentSquad { get; set; }
    }
}
