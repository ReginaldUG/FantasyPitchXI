namespace FantasyPitchXI.Models
{
    public class FantasyTeamPlayer
    {
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public int FantasyTeamId { get; set; }
        public virtual FantasyTeam FantasyTeam { get; set; }
        public ICollection<FantasyTeamLineup> FantasyTeamLineups { get; set; } = new List<FantasyTeamLineup>();
    }
}
