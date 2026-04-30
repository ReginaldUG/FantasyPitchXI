using FantasyPitchXI.Models;

namespace FantasyPitchXI.Data.Seed
{
    public class PlayerCSVImporter
    {
        private readonly AppDbContext _db;
        public PlayerCSVImporter(AppDbContext db)
        {
            _db = db;
        }

        public void import(string filepath)
        {
            if(!File.Exists(filepath))
            {
                Console.WriteLine($"File not found: {filepath}");
                return;
            }

            var csv = File.ReadAllLines(filepath).Skip(1);
            foreach (var line in csv)
            {
                var parts = line.Split(",");

                var playerName = parts[1].Trim();
                var position = Enum.Parse<PlayerPosition>(parts[2].Trim());
                var price = decimal.Parse(parts[3].Trim());
                var clubId = int.Parse(parts[4].Trim());

                var player = new Player
                {
                    Name = playerName,
                    Position = position,
                    Price = price,
                    ClubId = clubId
                };

                _db.Player.Add(player);
            }
            _db.SaveChanges();
        }
    }
}
