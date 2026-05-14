namespace FantasyPitchXI.Models
{
    public class FantasyTeamLineup
    {
        public int Id { get; set; }
        public int FantasyTeamId { get; set; }
        public int PlayerId { get; set; }
        public virtual FantasyTeamPlayer FantasyTeamPlayer { get; set; }
        public int Gameweek { get; set; }
        public bool IsStarting { get; set; }
        public int BenchOrder { get; set; }
        public bool IsCaptain { get; set; }
        public bool IsViceCaptain { get; set; }
    }
}
