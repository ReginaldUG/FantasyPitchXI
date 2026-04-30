namespace FantasyPitchXI.Data
{
    public static class GameState
    {
        public static int CurrentGameweek { get; set; } = 1;

        public static bool AdvanceGameweek()
        {
            if (CurrentGameweek >= 38)
            {
                return false;
            }
            CurrentGameweek++;

            return true;
        }

        public static void ResetGameweek()
        {
            CurrentGameweek = 1;
        }
    }
}
