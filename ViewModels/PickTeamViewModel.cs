using FantasyPitchXI.DTO;
using FantasyPitchXI.Models;

namespace FantasyPitchXI.ViewModels
{
    public class PickTeamViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public List<Player> Squad { get; set; } = new();
        public List<LineupPlayerDTO> Starting { get; set; } = new();
        public List<Player> Bench { get; set; } = new();
        
        public List<int> StartingTeamPlayerIDs { get; set; } = new();
        public List<int> BenchPlayerIDs { get; set; } = new();
        public int CaptainID { get; set; }
        public int ViceCaptainID { get; set; }
    }
}
