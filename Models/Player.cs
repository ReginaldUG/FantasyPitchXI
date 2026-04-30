namespace FantasyPitchXI.Models
{
    public class Player
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public PlayerPosition Position { get; set; }
        public decimal Price { get; set; }
        public int ClubId { get; set; }

        //Navigation property
        public virtual Club Club { get; set; }

        public virtual ICollection<FantasyTeamPlayer> FantasyTeamPlayers { get; set; } = new List<FantasyTeamPlayer>();
    }
}
