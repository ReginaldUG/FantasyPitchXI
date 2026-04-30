namespace FantasyPitchXI.Models
{
    public class Club
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
