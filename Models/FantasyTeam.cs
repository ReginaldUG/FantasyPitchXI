namespace FantasyPitchXI.Models
{


    public class FantasyTeam
    {
        public int Id { get; set; }
        public required string TeamName { get; set; }
        public decimal Budget { get; set; }
        public int TransferAvailableThisGameweek { get; set; }
        public virtual ICollection<FantasyTeamPlayer> FantasyTeamPlayers { get; set; } = new List<FantasyTeamPlayer>();
    }
}
