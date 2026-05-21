using FantasyPitchXI.Models;

namespace FantasyPitchXI.DTO.Request_DTO
{
    public class ValidateAllSelectionRequestDTO
    {
        public List<int> SelectedXIPlayersIDs { get; set; }
        public List<int> SelectedBenchPlayersIDs { get; set; }
    }

    public class SelectStartingXIandBenchRequestDTO
    {
        public FantasyTeam Team { get; set; }
        public List<int> StartingIDs { get; set; }
        public List<int> BenchIDs { get; set; }
        public int CapID { get; set; }
        public int ViceID { get; set; }
    }

    public class UpdateXIandBenchRequestDTO
    {
        public int TeamID { get; set; }
        public List<Player> ChosenXIPlayers { get; set; }
        public List<Player> SelectedBenchPlayers { get; set; }
        public int CapID { get; set; }
        public int ViceID { get; set; }
    }

    public class ValidateCapValidateViceRequestDTO
    {
        public FantasyTeam Team { get; set; }
        public List<Player> ChosenXIPlayers { get; set; }
        public int CapID { get; set; }
        public int ViceID { get; set; }
    }
}
