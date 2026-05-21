using FantasyPitchXI.Models;

namespace FantasyPitchXI.DTO.Request_DTO
{
    public class UpdateFantasyTeamRequestDTO
    {
        public FantasyTeam Team { get; set; }
        public List<int> PlayersOutIDs { get; set; }
        public List<int> PlayersInIDs { get; set; } 
        public List<Player> UpdatedSquad { get; set; }
        public int TransferCount { get; set; }
    }

    public class TransferPlayersRequestDTO
    {
        public FantasyTeam Team { get; set; }
        public List<int> UpdatedSquadPlayerIDs { get; set; }
    }
}
