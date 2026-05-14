namespace FantasyPitchXI.Data
{
    public static class GameRuleConstants
    {
        // Total Budget for each Fantasy Team
        public const decimal InitialBudget = 100m;

        //Total Gameweeks
        public const int TotalGameweeks = 38;

        // Max Players you can seleect from each club
        public const int MaxPlayersFromSameClub = 3;

        public const int TotalSquadSize = 15;
        public const int TotalSquadGoalkeepers = 2;
        public const int TotalSquadDefenders = 5;
        public const int TotalSquadMidfielders = 5;
        public const int TotalSquadStrikers = 3;

        //STARTING XI RULES
        public const int TotalXISize = 11;
        public const int MaxGoalkeeperXI = 1;
        public const int MinDefendersXI = 3;
        public const int MinMidfieldersXI = 2;
        public const int MinStrikersXI = 1;


        // Max number of transfers allowed in a gameweek (fpl standard)
        public const int MaxTransfersPerGW = 1;

        public const int ERRORValue = -999;
    }
}
