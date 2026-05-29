using FantasyPitchXI.Models;
namespace FantasyPitchXI.DTO;


public class LineupPlayerDTO
{
    public required Player Player { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsViceCaptain { get; set; }
    
}